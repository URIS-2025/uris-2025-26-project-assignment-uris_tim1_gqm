using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Application.Interfaces.Clients;
using Shared.Auth;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IAuditClient _auditClient;

    public AuthController(IAuthService authService, IAuditClient auditClient)
    {
        _authService = authService;
        _auditClient = auditClient;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        _ = _auditClient.LogAsync(Guid.Empty, "Anonymous", "UserLoggedIn", "User", Guid.Empty, new { email = request.Email });
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var response = await _authService.RefreshTokenAsync(request);
        return Ok(response);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserContextResponse>> GetCurrentUser()
    {
        var userId = User.GetUserId();
        var response = await _authService.GetCurrentUserAsync(userId);
        return Ok(response);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        var userId = User.GetUserId();
        await _authService.LogoutAsync(userId);
        return NoContent();
    }

    [Authorize]
    [HttpPut("change-password")]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.GetUserId();
        await _authService.ChangePasswordAsync(userId, request);
        _ = _auditClient.LogAsync(userId, "User", "PasswordChanged", "User", userId);
        return NoContent();
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _authService.ForgotPasswordAsync(request);
        _ = _auditClient.LogAsync(Guid.Empty, "Anonymous", "PasswordResetRequested", "User", Guid.Empty, new { email = request.Email });
        return NoContent();
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        await _authService.ResetPasswordAsync(request);
        return Ok();
    }
}
