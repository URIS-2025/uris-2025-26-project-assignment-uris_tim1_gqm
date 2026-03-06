using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Application.Interfaces.Clients;
using Shared.Contracts;

namespace UserService.API.Controllers;

[ApiController]
[Route("users")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuditClient _auditClient;

    public UserController(IUserService userService, IAuditClient auditClient)
    {
        _userService = userService;
        _auditClient = auditClient;
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResponse<UserResponse>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        var response = await _userService.GetAllAsync(page, size);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetById(Guid id)
    {
        var response = await _userService.GetByIdAsync(id);
        return Ok(response);
    }

    [HttpGet("email/{email}")]
    public async Task<ActionResult<UserResponse>> GetByEmail(string email)
    {
        var response = await _userService.GetByEmailAsync(email);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create([FromBody] UserRequest request)
    {
        var response = await _userService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserResponse>> UpdateProfile(Guid id, [FromBody] UpdateProfileRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        if (userId != id)
            return Forbid();

        var response = await _userService.UpdateProfileAsync(id, request);
        return Ok(response);
    }

    [Authorize(Roles = $"{Domain.Constants.Roles.SystemAdmin},{Domain.Constants.Roles.OrganizationAdmin}")]
    [HttpPut("{id:guid}/toggle-active")]
    public async Task<ActionResult<UserResponse>> ToggleIsActive(Guid id)
    {
        var response = await _userService.ToggleIsActiveAsync(id);
        var actorId = Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var aid) ? aid : Guid.Empty;
        var action = response.IsActive ? "UserActivated" : "UserDeactivated";
        _ = _auditClient.LogAsync(actorId, "Admin", action, "User", id);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _userService.DeleteAsync(id);
        return NoContent();
    }
}
