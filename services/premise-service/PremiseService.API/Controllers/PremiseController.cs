using Microsoft.AspNetCore.Mvc;
using PremiseService.Application.DTOs;
using PremiseService.Application.Interfaces;

namespace PremiseService.API.Controllers;

[ApiController]
[Route("[controller]")]
public class PremiseController : ControllerBase
{
    private readonly IPremiseService _premiseService;

    public PremiseController(IPremiseService premiseService)
    {
        _premiseService = premiseService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PremiseResponse>>> GetAllActive()
    {
        var premises = await _premiseService.GetAllActiveAsync();
        return Ok(premises);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PremiseResponse>> GetById(Guid id)
    {
        var premise = await _premiseService.GetByIdAsync(id);
        return Ok(premise);
    }

    [HttpGet("goal/{goalId:guid}")]
    public async Task<ActionResult<IEnumerable<PremiseResponse>>> GetByGoalId(Guid goalId)
    {
        var premises = await _premiseService.GetByGoalIdAsync(goalId);
        return Ok(premises);
    }

    [HttpGet("strategy/{strategyId:guid}")]
    public async Task<ActionResult<IEnumerable<PremiseResponse>>> GetByStrategyId(Guid strategyId)
    {
        var premises = await _premiseService.GetByStrategyIdAsync(strategyId);
        return Ok(premises);
    }

    [HttpGet("{id:guid}/history")]
    public async Task<ActionResult<IEnumerable<PremiseResponse>>> GetVersionHistory(Guid id)
    {
        var history = await _premiseService.GetVersionHistoryAsync(id);
        return Ok(history);
    }

    [HttpPost]
    public async Task<ActionResult<PremiseResponse>> Create([FromBody] PremiseRequest request)
    {
        var created = await _premiseService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PremiseResponse>> Update(Guid id, [FromBody] PremiseUpdateRequest request)
    {
        var updated = await _premiseService.UpdateAsync(id, request);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        await _premiseService.DeactivateAsync(id);
        return NoContent();
    }
}
