using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using Shared.Auth;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<ActionResult<List<RoleResponse>>> GetAll()
    {
        var response = await _roleService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RoleResponse>> GetById(Guid id)
    {
        var response = await _roleService.GetByIdAsync(id);
        return Ok(response);
    }

    [HttpPost]
    [RequirePermission("manage_roles")]
    public async Task<ActionResult<RoleResponse>> Create([FromBody] RoleRequest request)
    {
        var response = await _roleService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("manage_roles")]
    public async Task<ActionResult<RoleResponse>> Update(Guid id, [FromBody] RoleRequest request)
    {
        var response = await _roleService.UpdateAsync(id, request);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("manage_roles")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _roleService.DeleteAsync(id);
        return NoContent();
    }
}
