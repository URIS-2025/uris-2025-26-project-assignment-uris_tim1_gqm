using Microsoft.AspNetCore.Mvc;
using GQMGoalService.Application.DTOs;
using GQMGoalService.Application.DTOs.GqmGoal;
using GQMGoalService.Application.Interfaces;

namespace GQMGoalService.API.Controllers;

[ApiController]
[Route("[controller]")]
public class GqmGoalController : ControllerBase
{
    private readonly IGqmGoalService _service;

    public GqmGoalController(IGqmGoalService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<GqmGoalResponse>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(pageNumber, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GqmGoalResponse>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("by-goal/{goalId}")]
    public async Task<ActionResult<IEnumerable<GqmGoalResponse>>> GetByGoalId(Guid goalId, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByGoalIdAsync(goalId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<GqmGoalResponse>> Create([FromBody] GqmGoalRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<GqmGoalResponse>> Update(Guid id, [FromBody] GqmGoalRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
