using Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GQMGoalService.Application.DTOs;
using GQMGoalService.Application.DTOs.GqmGoal;
using GQMGoalService.Application.Interfaces;
using GQMGoalService.Application.Interfaces.Clients;
using Shared.Auth;

namespace GQMGoalService.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class GqmGoalController : ControllerBase
{
    private readonly IGqmGoalService _service;
    private readonly IAuditClient _auditClient;

    public GqmGoalController(IGqmGoalService service, IAuditClient auditClient)
    {
        _service = service;
        _auditClient = auditClient;
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResponse<GqmGoalResponse>>> GetAll([FromQuery] PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(request, cancellationToken);
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
    [RequirePermission("manage_gqm_goals")]
    public async Task<ActionResult<GqmGoalResponse>> Create([FromBody] GqmGoalRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        _ = _auditClient.LogAsync(Guid.Empty, "System", "GqmGoalCreated", "GqmGoal", result.Id, new { result.GoalId });
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [RequirePermission("manage_gqm_goals")]
    public async Task<ActionResult<GqmGoalResponse>> Update(Guid id, [FromBody] GqmGoalRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [RequirePermission("manage_gqm_goals")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
