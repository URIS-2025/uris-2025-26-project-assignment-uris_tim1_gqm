using DepartmentService.Application.DTOs;
using Shared.Contracts;
using DepartmentService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DepartmentService.API.Controllers;

[ApiController]
[Route("organizations")]
public class OrganizationController : ControllerBase
{
    private readonly IOrganizationService _organizationService;

    public OrganizationController(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResponse<OrganizationResponse>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20)
    {
        var result = await _organizationService.GetAllAsync(page, size);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrganizationResponse>> GetById(Guid id)
    {
        var result = await _organizationService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<OrganizationResponse>> Create([FromBody] OrganizationRequest request)
    {
        var result = await _organizationService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<OrganizationResponse>> Update(Guid id, [FromBody] OrganizationRequest request)
    {
        var result = await _organizationService.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _organizationService.DeleteAsync(id);
        return NoContent();
    }
}
