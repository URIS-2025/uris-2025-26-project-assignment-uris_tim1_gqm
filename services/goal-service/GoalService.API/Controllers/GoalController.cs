using FluentValidation;
using GoalService.Application.DTOs;
using Shared.Contracts;
using GoalService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Auth;

namespace GoalService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GoalController : ControllerBase
{
    private readonly IGoalService _goalService;
    private readonly IValidator<GoalRequest> _validator;

    public GoalController(IGoalService goalService, IValidator<GoalRequest> validator)
    {
        _goalService = goalService;
        _validator = validator;
    }

    /// <summary>
    /// Get all goals with their strategies and influences.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginationResponse<GoalResponse>>> GetAll([FromQuery] PaginationRequest request)
    {
        var paginatedGoals = await _goalService.GetAllPaginatedAsync(request);
        return Ok(paginatedGoals);
    }

    /// <summary>
    /// Get a specific goal by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GoalResponse>> GetById(Guid id)
    {
        var goal = await _goalService.GetByIdAsync(id);
        if (goal is null)
            return NotFound(new { message = $"Goal with ID '{id}' was not found." });

        return Ok(goal);
    }

    /// <summary>
    /// Get a specific goal by ID along with its related external data (Premises, Assessments, QGM Goals).
    /// </summary>
    [HttpGet("{id:guid}/details")]
    public async Task<ActionResult<GoalDetailsResponse>> GetDetails(Guid id)
    {
        var details = await _goalService.GetGoalDetailsAsync(id);
        if (details is null)
            return NotFound(new { message = $"Goal with ID '{id}' was not found." });

        return Ok(details);
    }

    /// <summary>
    /// Get all goals belonging to a specific department.
    /// </summary>
    [HttpGet("department/{departmentId:guid}")]
    public async Task<ActionResult<IEnumerable<GoalResponse>>> GetByDepartmentId(Guid departmentId)
    {
        var goals = await _goalService.GetByDepartmentIdAsync(departmentId);
        return Ok(goals);
    }

    /// <summary>
    /// Create a new goal.
    /// </summary>
    [HttpPost]
    [RequirePermission("create_goals")]
    public async Task<ActionResult<GoalResponse>> Create([FromBody] GoalRequest request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

        var goal = await _goalService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = goal.Id }, goal);
    }

    /// <summary>
    /// Update an existing goal.
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission("edit_goals")]
    public async Task<ActionResult<GoalResponse>> Update(Guid id, [FromBody] GoalRequest request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

        var goal = await _goalService.UpdateAsync(id, request);
        if (goal is null)
            return NotFound(new { message = $"Goal with ID '{id}' was not found." });

        return Ok(goal);
    }

    /// <summary>
    /// Delete a goal and all its strategies and influences (cascade).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission("delete_goals")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _goalService.DeleteAsync(id);
        if (!deleted)
            return NotFound(new { message = $"Goal with ID '{id}' was not found." });

        return NoContent();
    }
}
