using DepartmentService.Application.DTOs;
using DepartmentService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DepartmentService.API.Controllers;

[ApiController]
[Route("departments")]
[Authorize]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<DepartmentResponse>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20)
    {
        var result = await _departmentService.GetAllAsync(page, size);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DepartmentResponse>> GetById(Guid id)
    {
        var result = await _departmentService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpGet("organization/{organizationId:guid}")]
    public async Task<ActionResult<PagedResponse<DepartmentResponse>>> GetByOrganizationId(
        Guid organizationId,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20)
    {
        var result = await _departmentService.GetByOrganizationIdAsync(organizationId, page, size);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<DepartmentResponse>> Create([FromBody] DepartmentRequest request)
    {
        var result = await _departmentService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DepartmentResponse>> Update(Guid id, [FromBody] DepartmentRequest request)
    {
        var result = await _departmentService.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _departmentService.DeleteAsync(id);
        return NoContent();
    }
}
