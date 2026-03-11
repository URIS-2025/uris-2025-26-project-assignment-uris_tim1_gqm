using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PremiseService.Application.DTOs;
using PremiseService.Application.Interfaces;
using PremiseService.Application.Interfaces.Clients;
using Shared.Auth;
using Shared.Contracts;

namespace PremiseService.API.Controllers;


[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PremiseController : ControllerBase
{
    private readonly IPremiseService _premiseService;
    private readonly IAuditClient _auditClient;

    public PremiseController(IPremiseService premiseService, IAuditClient auditClient)
    {
        _premiseService = premiseService;
        _auditClient = auditClient;
    }


    /// <summary>Returns a paginated list of all premises.</summary>
    [HttpGet]
    public async Task<ActionResult<PaginationResponse<PremiseResponse>>> GetAll(
        [FromQuery] PaginationRequest request)
    {
        var result = await _premiseService.GetAllAsync(request);
        return Ok(result);
    }


    /// <summary>Returns a single premise by its unique identifier.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PremiseResponse>> GetById(Guid id)
    {
        var premise = await _premiseService.GetByIdAsync(id);
        return Ok(premise);
    }


    /// <summary>Returns active premises associated with a specific goal.</summary>
    [HttpGet("active/goal/{goalId:guid}")]
    public async Task<ActionResult<IEnumerable<PremiseActiveResponse>>> GetActiveByGoal(Guid goalId)
    {
        var premises = await _premiseService.GetActiveByGoalIdAsync(goalId);
        return Ok(premises);
    }


    /// <summary>Returns active premises associated with a specific strategy.</summary>
    [HttpGet("active/strategy/{strategyId:guid}")]
    public async Task<ActionResult<IEnumerable<PremiseActiveResponse>>> GetActiveByStrategy(Guid strategyId)
    {
        var premises = await _premiseService.GetActiveByStrategyIdAsync(strategyId);
        return Ok(premises);
    }


    /// <summary>Creates a new premise.</summary>
    [HttpPost]
    [RequirePermission("manage_premises")]
    public async Task<ActionResult<PremiseResponse>> Create([FromBody] PremiseRequest request)
    {
        var created = await _premiseService.CreateAsync(request);
        _ = _auditClient.LogAsync(Guid.Empty, "System", "PremiseCreated", "Premise", created.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }


    /// <summary>Updates a premise by creating a new version and deactivating the old one.</summary>
    [HttpPut("{id:guid}")]
    [RequirePermission("manage_premises")]
    public async Task<ActionResult<PremiseResponse>> Update(Guid id, [FromBody] PremiseUpdateRequest request)
    {
        var updated = await _premiseService.UpdateAsync(id, request);
        _ = _auditClient.LogAsync(Guid.Empty, "System", "PremiseVersioned", "Premise", updated.Id, new { previousId = id });
        return Ok(updated);
    }


    /// <summary>Soft-deletes a premise by setting IsActive to false.</summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission("manage_premises")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _premiseService.DeleteAsync(id);
        _ = _auditClient.LogAsync(Guid.Empty, "System", "PremiseDeactivated", "Premise", id);
        return NoContent();
    }

    /// <summary>Soft-deletes premises associated with a goal.</summary>
    [HttpDelete("goal/{goalId:guid}")]
    [RequirePermission("manage_premises")]
    public async Task<IActionResult> DeleteByGoalId(Guid goalId)
    {
        await _premiseService.DeleteByGoalIdAsync(goalId);
        _ = _auditClient.LogAsync(Guid.Empty, "System", "PremisesDeactivatedForGoal", "Premise", goalId);
        return NoContent();
    }
}
