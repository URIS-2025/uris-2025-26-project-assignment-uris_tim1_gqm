using FluentValidation;
using GoalService.Application.DTOs;
using GoalService.Application.Interfaces;
using GoalService.Application.Interfaces.Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Auth;

namespace GoalService.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class StrategyController : ControllerBase
{
    private readonly IStrategyService _strategyService;
    private readonly IValidator<StrategyRequest> _validator;
    private readonly IAuditClient _auditClient;

    public StrategyController(IStrategyService strategyService, IValidator<StrategyRequest> validator, IAuditClient auditClient)
    {
        _strategyService = strategyService;
        _validator = validator;
        _auditClient = auditClient;
    }

    /// <summary>
    /// Get all strategies for a specific goal.
    /// </summary>
    [HttpGet("goal/{goalId:guid}")]
    public async Task<ActionResult<IEnumerable<StrategyResponse>>> GetByGoalId(Guid goalId)
    {
        var strategies = await _strategyService.GetByGoalIdAsync(goalId);
        return Ok(strategies);
    }

    /// <summary>
    /// Get a specific strategy by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StrategyResponse>> GetById(Guid id)
    {
        var strategy = await _strategyService.GetByIdAsync(id);
        if (strategy is null)
            return NotFound(new { message = $"Strategy with ID '{id}' was not found." });

        return Ok(strategy);
    }

    /// <summary>
    /// Create a new strategy for a goal.
    /// </summary>
    [HttpPost]
    [RequirePermission("create_goals")]
    public async Task<ActionResult<StrategyResponse>> Create([FromBody] StrategyRequest request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

        var strategy = await _strategyService.CreateAsync(request);
        _ = _auditClient.LogAsync(Guid.Empty, "System", "StrategyCreated", "Strategy", strategy.Id, new { strategy.GoalId });
        return CreatedAtAction(nameof(GetById), new { id = strategy.Id }, strategy);
    }

    /// <summary>
    /// Update an existing strategy.
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission("edit_goals")]
    public async Task<ActionResult<StrategyResponse>> Update(Guid id, [FromBody] StrategyRequest request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

        var strategy = await _strategyService.UpdateAsync(id, request);
        if (strategy is null)
            return NotFound(new { message = $"Strategy with ID '{id}' was not found." });

        return Ok(strategy);
    }

    /// <summary>
    /// Delete a strategy and its influences (cascade).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission("delete_goals")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _strategyService.DeleteAsync(id);
        if (!deleted)
            return NotFound(new { message = $"Strategy with ID '{id}' was not found." });

        return NoContent();
    }
}
