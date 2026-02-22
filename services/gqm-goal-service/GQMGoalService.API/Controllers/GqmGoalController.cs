using Microsoft.AspNetCore.Mvc;
using GQMGoalService.Application.DTOs;
using GQMGoalService.Application.DTOs.GqmGoal;
using GQMGoalService.Application.Interfaces;

namespace GQMGoalService.API.Controllers;

[ApiController]
[Route("api/v1/GQM-goal/[controller]")]
public class GqmGoalController : ControllerBase
{
    private readonly IGqmGoalService _service;

    public GqmGoalController(IGqmGoalService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<GqmGoalResponse>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetAllAsync(pageNumber, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GqmGoalResponse>> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpGet("by-goal/{goalId}")]
    public async Task<ActionResult<IEnumerable<GqmGoalResponse>>> GetByGoalId(Guid goalId)
    {
        var result = await _service.GetByGoalIdAsync(goalId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<GqmGoalResponse>> Create([FromBody] GqmGoalRequest request)
    {
        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<GqmGoalResponse>> Update(Guid id, [FromBody] GqmGoalRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
