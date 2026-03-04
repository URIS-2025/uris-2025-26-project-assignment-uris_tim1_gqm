using UserService.API.Controllers;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace UserService.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _serviceMock;
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _serviceMock = new Mock<IAuthService>();
        _sut = new AuthController(_serviceMock.Object);
    }

    [Fact]
    public async Task Login_ReturnsOk_WithLoginResponse()
    {
        var request = new LoginRequest { Email = "admin@test.com", Password = "Password@1" };
        var response = new LoginResponse { AccessToken = "token", RefreshToken = "refresh", ExpiresAt = DateTime.UtcNow.AddMinutes(15) };
        _serviceMock.Setup(s => s.LoginAsync(request)).ReturnsAsync(response);

        var result = await _sut.Login(request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value.Should().BeOfType<LoginResponse>().Subject;
        value.AccessToken.Should().Be("token");
    }

    [Fact]
    public async Task Logout_ReturnsNoContent()
    {
        _serviceMock.Setup(s => s.LogoutAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);
        
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        }, "Test"));
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var result = await _sut.Logout();

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task ChangePassword_ReturnsNoContent()
    {
        var request = new ChangePasswordRequest
        {
            OldPassword = "Old@1234",
            NewPassword = "New@1234!",
            ConfirmNewPassword = "New@1234!"
        };
        _serviceMock.Setup(s => s.ChangePasswordAsync(It.IsAny<Guid>(), request))
            .Returns(Task.CompletedTask);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        }, "Test"));
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var result = await _sut.ChangePassword(request);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task RefreshToken_ReturnsOk_WithRefreshResponse()
    {
        var request = new RefreshTokenRequest { RefreshToken = "old-token" };
        var response = new RefreshTokenResponse { AccessToken = "new-token", ExpiresAt = DateTime.UtcNow.AddMinutes(15) };
        _serviceMock.Setup(s => s.RefreshTokenAsync(request)).ReturnsAsync(response);

        var result = await _sut.RefreshToken(request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value.Should().BeOfType<RefreshTokenResponse>().Subject;
        value.AccessToken.Should().Be("new-token");
    }

    [Fact]
    public async Task ForgotPassword_ReturnsOk()
    {
        var request = new ForgotPasswordRequest { Email = "test@test.com" };
        _serviceMock.Setup(s => s.ForgotPasswordAsync(request)).Returns(Task.CompletedTask);

        var result = await _sut.ForgotPassword(request);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task ResetPassword_ReturnsOk()
    {
        var request = new ResetPasswordRequest { Token = "token", NewPassword = "New@1234", ConfirmNewPassword = "New@1234" };
        _serviceMock.Setup(s => s.ResetPasswordAsync(request)).Returns(Task.CompletedTask);

        var result = await _sut.ResetPassword(request);

        result.Should().BeOfType<OkResult>();
    }
}
