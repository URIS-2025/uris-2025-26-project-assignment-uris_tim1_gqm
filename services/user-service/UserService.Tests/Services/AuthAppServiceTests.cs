using Microsoft.Extensions.Configuration;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Application.Interfaces.Clients;
using UserService.Application.Services;
using UserService.Application.Validators;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Tests.Helpers;
using FluentAssertions;
using FluentValidation;
using Moq;

namespace UserService.Tests.Services;

public class AuthAppServiceTests : IDisposable
{
    private readonly Infrastructure.Persistence.UserServiceDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IDepartmentClient> _departmentClientMock;
    private readonly AuthAppService _sut;

    public AuthAppServiceTests()
    {
        _context = TestDbContextFactory.Create();

        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:SecretKey", "test-secret-key-that-is-at-least-32-chars-long!!" },
            { "Jwt:Issuer", "test-issuer" },
            { "Jwt:Audience", "test-audience" },
            { "Jwt:ExpiryMinutes", "15" },
            { "App:FrontendBaseUrl", "http://localhost:3000" }
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _emailServiceMock = new Mock<IEmailService>();
        _departmentClientMock = new Mock<IDepartmentClient>();

        _sut = new AuthAppService(
            _context,
            _configuration,
            _emailServiceMock.Object,
            _departmentClientMock.Object,
            new ChangePasswordRequestValidator(),
            new ResetPasswordRequestValidator());
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private async Task<User> SeedUser(string email = "test@example.com", string password = "Password@1")
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    // ── LoginAsync ──

    [Fact]
    public async Task LoginAsync_ReturnsTokens_WhenCredentialsValid()
    {
        await SeedUser();

        var result = await _sut.LoginAsync(new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password@1"
        });

        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_ThrowsBadRequest_WhenEmailInvalid()
    {
        var act = () => _sut.LoginAsync(new LoginRequest
        {
            Email = "wrong@test.com",
            Password = "Password@1"
        });

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task LoginAsync_ThrowsBadRequest_WhenPasswordWrong()
    {
        await SeedUser();

        var act = () => _sut.LoginAsync(new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword@1"
        });

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task LoginAsync_ThrowsBadRequest_WhenUserDeactivated()
    {
        var user = await SeedUser();
        user.IsActive = false;
        await _context.SaveChangesAsync();

        var act = () => _sut.LoginAsync(new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password@1"
        });

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*deactivated*");
    }

    // ── RefreshTokenAsync ──

    [Fact]
    public async Task RefreshTokenAsync_ReturnsNewAccessToken()
    {
        var user = await SeedUser();
        var loginResult = await _sut.LoginAsync(new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password@1"
        });

        var result = await _sut.RefreshTokenAsync(new RefreshTokenRequest
        {
            RefreshToken = loginResult.RefreshToken
        });

        result.AccessToken.Should().NotBeNullOrEmpty();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task RefreshTokenAsync_ThrowsBadRequest_WhenTokenInvalid()
    {
        var act = () => _sut.RefreshTokenAsync(new RefreshTokenRequest
        {
            RefreshToken = "invalid-token"
        });

        await act.Should().ThrowAsync<BadRequestException>();
    }

    // ── LogoutAsync ──

    [Fact]
    public async Task LogoutAsync_ClearsRefreshToken()
    {
        var user = await SeedUser();
        await _sut.LoginAsync(new LoginRequest { Email = "test@example.com", Password = "Password@1" });

        await _sut.LogoutAsync(user.Id);

        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser!.RefreshToken.Should().BeNull();
        updatedUser.RefreshTokenExpiry.Should().BeNull();
    }

    [Fact]
    public async Task LogoutAsync_ThrowsNotFoundException_WhenUserNotFound()
    {
        var act = () => _sut.LogoutAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── ChangePasswordAsync ──

    [Fact]
    public async Task ChangePasswordAsync_ChangesPassword_WhenOldPasswordCorrect()
    {
        var user = await SeedUser("user@test.com", "OldPassword@1");

        await _sut.ChangePasswordAsync(user.Id, new ChangePasswordRequest
        {
            OldPassword = "OldPassword@1",
            NewPassword = "NewPassword@1!",
            ConfirmNewPassword = "NewPassword@1!"
        });

        var updatedUser = await _context.Users.FindAsync(user.Id);
        BCrypt.Net.BCrypt.Verify("NewPassword@1!", updatedUser!.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task ChangePasswordAsync_ThrowsBadRequest_WhenOldPasswordWrong()
    {
        var user = await SeedUser("user@test.com", "OldPassword@1");

        var act = () => _sut.ChangePasswordAsync(user.Id, new ChangePasswordRequest
        {
            OldPassword = "WrongOld@1",
            NewPassword = "NewPassword@1!",
            ConfirmNewPassword = "NewPassword@1!"
        });

        await act.Should().ThrowAsync<BadRequestException>();
    }

    // ── ForgotPasswordAsync ──

    [Fact]
    public async Task ForgotPasswordAsync_SetsResetToken_WhenEmailExists()
    {
        var user = await SeedUser();

        await _sut.ForgotPasswordAsync(new ForgotPasswordRequest { Email = "test@example.com" });

        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser!.PasswordResetToken.Should().NotBeNullOrEmpty();
        updatedUser.PasswordResetTokenExpiry.Should().BeAfter(DateTime.UtcNow);
        _emailServiceMock.Verify(e => e.SendPasswordResetEmailAsync("test@example.com", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ForgotPasswordAsync_DoesNotThrow_WhenEmailNotFound()
    {
        var act = () => _sut.ForgotPasswordAsync(new ForgotPasswordRequest { Email = "nonexistent@test.com" });

        await act.Should().NotThrowAsync();
        _emailServiceMock.Verify(e => e.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    // ── ResetPasswordAsync ──

    [Fact]
    public async Task ResetPasswordAsync_ResetsPassword_WhenTokenValid()
    {
        var user = await SeedUser();

        // Capture the plain token from the reset link via Moq Callback
        string capturedResetLink = string.Empty;
        _emailServiceMock
            .Setup(e => e.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>((email, link) => capturedResetLink = link)
            .Returns(Task.CompletedTask);

        await _sut.ForgotPasswordAsync(new ForgotPasswordRequest { Email = "test@example.com" });

        // Extract the plain token from the captured URL: ...?token=<plain_token>
        var plainToken = capturedResetLink.Split("?token=")[1];

        await _sut.ResetPasswordAsync(new ResetPasswordRequest
        {
            Token = plainToken,
            NewPassword = "ResetPassword@1!",
            ConfirmNewPassword = "ResetPassword@1!"
        });

        var updatedUser = await _context.Users.FindAsync(user.Id);
        BCrypt.Net.BCrypt.Verify("ResetPassword@1!", updatedUser!.PasswordHash).Should().BeTrue();
        updatedUser.PasswordResetToken.Should().BeNull();
        updatedUser.PasswordResetTokenExpiry.Should().BeNull();
    }

    [Fact]
    public async Task ResetPasswordAsync_ThrowsBadRequest_WhenTokenInvalid()
    {
        var act = () => _sut.ResetPasswordAsync(new ResetPasswordRequest
        {
            Token = "invalid-token",
            NewPassword = "NewPassword@1!",
            ConfirmNewPassword = "NewPassword@1!"
        });

        await act.Should().ThrowAsync<BadRequestException>();
    }
}
