using Microsoft.AspNetCore.Mvc;
using PremiseService.Application.DTOs;
using PremiseService.Application.Interfaces;

namespace PremiseService.API.Controllers;


[ApiController]
[Route("premises")]
public class PremiseController : ControllerBase
{
    private readonly IPremiseService _premiseService;

    public PremiseController(IPremiseService premiseService)
    {
        _premiseService = premiseService;
    }


    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<PremiseResponse>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var result = await _premiseService.GetAllAsync(page, size);
        return Ok(result);
    }


    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PremiseResponse>> GetById(Guid id)
    {
        var premise = await _premiseService.GetByIdAsync(id);
        return Ok(premise);
    }


    [HttpGet("active/goal/{goalId:guid}")]
    public async Task<ActionResult<IEnumerable<PremiseActiveResponse>>> GetActiveByGoal(Guid goalId)
    {
        var premises = await _premiseService.GetActiveByGoalIdAsync(goalId);
        return Ok(premises);
    }


    [HttpGet("active/strategy/{strategyId:guid}")]
    public async Task<ActionResult<IEnumerable<PremiseActiveResponse>>> GetActiveByStrategy(Guid strategyId)
    {
        var premises = await _premiseService.GetActiveByStrategyIdAsync(strategyId);
        return Ok(premises);
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
    public async Task<IActionResult> Delete(Guid id)
    {
        await _premiseService.DeleteAsync(id);
        return NoContent();
    }
}
