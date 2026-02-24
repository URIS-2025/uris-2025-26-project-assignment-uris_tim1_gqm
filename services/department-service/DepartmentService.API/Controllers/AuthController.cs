using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace DepartmentService.API.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public AuthController(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    /// <summary>
    /// DEV ONLY — Generates a test JWT token for API testing.
    /// </summary>
    [HttpPost("dev-token")]
    [AllowAnonymous]
    public IActionResult GenerateDevToken()
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var secretKey = _configuration["JwtSettings:SecretKey"]
            ?? _configuration["JWT_SECRET_KEY"]
            ?? throw new InvalidOperationException("JWT secret key not configured.");

        var issuer = _configuration["JwtSettings:Issuer"] ?? "GqmPlus";
        var audience = _configuration["JwtSettings:Audience"] ?? "GqmPlus";

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, "dev-user"),
            new Claim(ClaimTypes.Email, "dev@gqmplus.local"),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new
        {
            token = tokenString,
            expiresAt = token.ValidTo,
            usage = "Copy the token and paste it into Swagger's 'Authorize' box as: Bearer <token>"
        });
    }
}
