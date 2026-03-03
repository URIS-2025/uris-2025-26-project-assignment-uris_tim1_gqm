using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using FluentValidation;

namespace UserService.Application.Services;

public class AuthAppService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;
    private readonly IValidator<ResetPasswordRequest> _resetPasswordValidator;

    public AuthAppService(
        IApplicationDbContext context,
        IConfiguration configuration,
        IEmailService emailService,
        IValidator<ChangePasswordRequest> changePasswordValidator,
        IValidator<ResetPasswordRequest> resetPasswordValidator)
    {
        _context = context;
        _configuration = configuration;
        _emailService = emailService;
        _changePasswordValidator = changePasswordValidator;
        _resetPasswordValidator = resetPasswordValidator;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new BadRequestException("Invalid email or password.");

        if (!user.IsActive)
            throw new BadRequestException("User account is deactivated.");

        var (roleNames, permissionNames) = await GetUserClaimsAsync(user.Id);

        var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "15");
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);
        var accessToken = GenerateJwtToken(user, roleNames, permissionNames, expiresAt);

        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt
        };
    }

    public async Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var user = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

        if (user is null || user.RefreshTokenExpiry < DateTime.UtcNow)
            throw new BadRequestException("Invalid or expired refresh token.");

        if (!user.IsActive)
            throw new BadRequestException("User account is deactivated.");

        var (roleNames, permissionNames) = await GetUserClaimsAsync(user.Id);

        var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "15");
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);
        var accessToken = GenerateJwtToken(user, roleNames, permissionNames, expiresAt);

        user.RefreshToken = GenerateRefreshToken();
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new RefreshTokenResponse
        {
            AccessToken = accessToken,
            ExpiresAt = expiresAt
        };
    }

    public async Task LogoutAsync(Guid userId)
    {
        var user = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
            throw new NotFoundException(nameof(User), userId);

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var validationResult = await _changePasswordValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var user = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
            throw new NotFoundException(nameof(User), userId);

        if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
            throw new BadRequestException("Old password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null)
            return; // Do not reveal whether the email exists

        var plainToken = GenerateRefreshToken();
        user.PasswordResetToken = HashWithSha256(plainToken);
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var resetLink = $"{_configuration["App:BaseUrl"] ?? "https://localhost"}/reset-password?token={plainToken}";
        await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var validationResult = await _resetPasswordValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var hashedToken = HashWithSha256(request.Token);

        var user = await _context.Set<User>()
            .FirstOrDefaultAsync(u =>
                u.PasswordResetToken == hashedToken &&
                u.PasswordResetTokenExpiry > DateTime.UtcNow);

        if (user is null)
            throw new BadRequestException("Invalid or expired password reset token.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    private string GenerateJwtToken(User user, List<string> roles, List<string> permissions, DateTime expiresAt)
    {
        var secretKey = _configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey is not configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        foreach (var permission in permissions)
            claims.Add(new Claim("permission", permission));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private static string HashWithSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }

    private async Task<(List<string> roles, List<string> permissions)> GetUserClaimsAsync(Guid userId)
    {
        var assignments = await _context.Set<UserOrganizationRole>()
            .AsNoTracking()
            .Include(uor => uor.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .Where(uor => uor.UserId == userId)
            .ToListAsync();

        var roles = assignments
            .Select(uor => uor.Role.Name)
            .Distinct()
            .ToList();

        var permissions = assignments
            .SelectMany(uor => uor.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToList();

        return (roles, permissions);
    }
}
