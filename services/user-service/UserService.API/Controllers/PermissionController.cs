using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using Shared.Auth;

namespace UserService.API.Controllers;

[ApiController]
[Route("permissions")]
[Authorize]
public class PermissionController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpGet]
    public async Task<ActionResult<List<PermissionResponse>>> GetAll()
    {
        var response = await _permissionService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PermissionResponse>> GetById(Guid id)
    {
        var response = await _permissionService.GetByIdAsync(id);
        return Ok(response);
    }

    [HttpPost]
    [RequirePermission("manage_permissions")]
    public async Task<ActionResult<PermissionResponse>> Create([FromBody] PermissionRequest request)
    {
        var response = await _permissionService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("manage_permissions")]
    public async Task<ActionResult<PermissionResponse>> Update(Guid id, [FromBody] PermissionRequest request)
    {
        var response = await _permissionService.UpdateAsync(id, request);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("manage_permissions")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _permissionService.DeleteAsync(id);
        return NoContent();
    }
}
