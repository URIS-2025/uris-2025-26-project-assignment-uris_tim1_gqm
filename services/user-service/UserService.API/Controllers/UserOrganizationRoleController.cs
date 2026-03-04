using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;

namespace UserService.API.Controllers;

[ApiController]
[Route("user-roles")]
public class UserOrganizationRoleController : ControllerBase
{
    private readonly IUserOrganizationRoleService _userOrganizationRoleService;

    public UserOrganizationRoleController(IUserOrganizationRoleService userOrganizationRoleService)
    {
        _userOrganizationRoleService = userOrganizationRoleService;
    }

    [HttpPost]
    public async Task<ActionResult<UserOrganizationRoleResponse>> AssignRole([FromBody] AssignRoleRequest request)
    {
        var response = await _userOrganizationRoleService.AssignRoleAsync(request);
        return Created(string.Empty, response);
    }

    [HttpDelete]
    public async Task<ActionResult> RemoveRole(
        [FromQuery] Guid userId,
        [FromQuery] Guid roleId,
        [FromQuery] Guid organizationId)
    {
        await _userOrganizationRoleService.RemoveRoleAsync(userId, roleId, organizationId);
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
