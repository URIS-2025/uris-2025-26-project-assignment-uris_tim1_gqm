using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Application.Interfaces.Clients;
using Shared.Auth;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class UserOrganizationRoleController : ControllerBase
{
    private readonly IUserOrganizationRoleService _userOrganizationRoleService;
    private readonly IAuditClient _auditClient;

    public UserOrganizationRoleController(IUserOrganizationRoleService userOrganizationRoleService, IAuditClient auditClient)
    {
        _userOrganizationRoleService = userOrganizationRoleService;
        _auditClient = auditClient;
    }

    [HttpPost]
    [RequirePermission("manage_user_roles")]
    public async Task<ActionResult<UserOrganizationRoleResponse>> AssignRole([FromBody] AssignRoleRequest request)
    {
        var response = await _userOrganizationRoleService.AssignRoleAsync(request);
        _ = _auditClient.LogAsync(Guid.Empty, "System", "RoleAssigned", "User", request.UserId,
            new { request.RoleId, request.OrganizationId });
        return Created(string.Empty, response);
    }

    [HttpDelete]
    [RequirePermission("manage_user_roles")]
    public async Task<ActionResult> RemoveRole(
        [FromQuery] Guid userId,
        [FromQuery] Guid roleId,
        [FromQuery] Guid organizationId)
    {
        await _userOrganizationRoleService.RemoveRoleAsync(userId, roleId, organizationId);
        _ = _auditClient.LogAsync(Guid.Empty, "System", "RoleRemoved", "User", userId,
            new { roleId, organizationId });
        return NoContent();
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<List<UserOrganizationRoleResponse>>> GetByUserId(Guid userId)
    {
        var response = await _userOrganizationRoleService.GetByUserIdAsync(userId);
        return Ok(response);
    }

    [HttpGet("user/{userId:guid}/organization/{organizationId:guid}")]
    public async Task<ActionResult<List<UserOrganizationRoleResponse>>> GetByUserAndOrganization(
        Guid userId, Guid organizationId)
    {
        var response = await _userOrganizationRoleService.GetByUserAndOrganizationAsync(userId, organizationId);
        return Ok(response);
    }
}
