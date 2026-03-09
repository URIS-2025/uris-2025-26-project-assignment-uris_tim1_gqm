using DepartmentService.Application.DTOs;
using Shared.Contracts;
using DepartmentService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Auth;

namespace DepartmentService.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResponse<DepartmentResponse>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20)
    {
        var orgId = User.GetOrganizationId();
        if (orgId.HasValue)
        {
            var result = await _departmentService.GetByOrganizationIdAsync(orgId.Value, page, size);
            return Ok(result);
        }

        var allResult = await _departmentService.GetAllAsync(page, size);
        return Ok(allResult);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DepartmentResponse>> GetById(Guid id)
    {
        var result = await _departmentService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpGet("organization/{organizationId:guid}")]
    public async Task<ActionResult<PaginationResponse<DepartmentResponse>>> GetByOrganizationId(
        Guid organizationId,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20)
    {
        var result = await _departmentService.GetByOrganizationIdAsync(organizationId, page, size);
        return Ok(result);
    }

    [HttpPost]
    [RequirePermission("manage_departments")]
    public async Task<ActionResult<DepartmentResponse>> Create([FromBody] DepartmentRequest request)
    {
        var result = await _departmentService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("manage_departments")]
    public async Task<ActionResult<DepartmentResponse>> Update(Guid id, [FromBody] DepartmentRequest request)
    {
        var result = await _departmentService.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("manage_departments")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _departmentService.DeleteAsync(id);
        return NoContent();
    }
}
