using FluentValidation;
using GoalService.Application.DTOs;
using GoalService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GoalService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GoalInfluenceController : ControllerBase
{
    private readonly IGoalInfluenceService _influenceService;
    private readonly IValidator<GoalInfluenceRequest> _validator;

    public GoalInfluenceController(IGoalInfluenceService influenceService, IValidator<GoalInfluenceRequest> validator)
    {
        _influenceService = influenceService;
        _validator = validator;
    }

    /// <summary>
    /// Get the influence record for a specific goal (if it arose from a strategy).
    /// </summary>
    [HttpGet("goal/{goalId:guid}")]
    public async Task<ActionResult<GoalInfluenceResponse>> GetByGoalId(Guid goalId)
    {
        var influence = await _influenceService.GetByGoalIdAsync(goalId);
        if (influence is null)
            return NotFound(new { message = $"No influence record found for Goal '{goalId}'." });

        return Ok(influence);
    }

    /// <summary>
    /// Get all influence records for a specific strategy (child goals that arose from it).
    /// </summary>
    [HttpGet("strategy/{strategyId:guid}")]
    public async Task<ActionResult<IEnumerable<GoalInfluenceResponse>>> GetByStrategyId(Guid strategyId)
    {
        var influences = await _influenceService.GetByStrategyIdAsync(strategyId);
        return Ok(influences);
    }

    /// <summary>
    /// Create a new goal influence (link a goal to a strategy it arose from).
    /// Validates: goal/strategy existence, no duplicate influence, no cycle in hierarchy.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<GoalInfluenceResponse>> Create([FromBody] GoalInfluenceRequest request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

        var influence = await _influenceService.CreateAsync(request);
        return CreatedAtAction(nameof(GetByGoalId), new { goalId = influence.GoalId }, influence);
    }

    /// <summary>
    /// Delete a goal influence record.
    /// </summary>
    [HttpDelete("{goalId:guid}")]
    public async Task<IActionResult> Delete(Guid goalId)
    {
        var deleted = await _influenceService.DeleteAsync(goalId);
        if (!deleted)
            return NotFound(new { message = $"No influence record found for Goal '{goalId}'." });

        return NoContent();
    }
}
