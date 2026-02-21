using Microsoft.AspNetCore.Mvc;
using PremiseService.Application.DTOs;
using PremiseService.Application.Interfaces;

namespace PremiseService.API.Controllers;

/// <summary>
/// REST API controller for managing premises within the GQM+ Strategy model.
/// Provides CRUD operations, filtering by goal/strategy, and version history.
/// </summary>
[ApiController]
[Route("[controller]")]
public class PremiseController : ControllerBase
{
    private readonly IPremiseService _premiseService;

    public PremiseController(IPremiseService premiseService)
    {
        _premiseService = premiseService;
    }

    /// <summary>Returns all currently active premises.</summary>
    /// <response code="200">List of active premises.</response>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PremiseResponse>>> GetAllActive()
    {
        var premises = await _premiseService.GetAllActiveAsync();
        return Ok(premises);
    }

    /// <summary>Returns a single premise by its unique identifier.</summary>
    /// <param name="id">The premise identifier.</param>
    /// <response code="200">The requested premise.</response>
    /// <response code="404">Premise not found.</response>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PremiseResponse>> GetById(Guid id)
    {
        var premise = await _premiseService.GetByIdAsync(id);
        return Ok(premise);
    }

    /// <summary>Returns all active premises associated with a specific goal.</summary>
    /// <param name="goalId">The goal identifier to filter by.</param>
    /// <response code="200">List of premises for the given goal.</response>
    [HttpGet("goal/{goalId:guid}")]
    public async Task<ActionResult<IEnumerable<PremiseResponse>>> GetByGoalId(Guid goalId)
    {
        var premises = await _premiseService.GetByGoalIdAsync(goalId);
        return Ok(premises);
    }

    /// <summary>Returns all active premises associated with a specific strategy.</summary>
    /// <param name="strategyId">The strategy identifier to filter by.</param>
    /// <response code="200">List of premises for the given strategy.</response>
    [HttpGet("strategy/{strategyId:guid}")]
    public async Task<ActionResult<IEnumerable<PremiseResponse>>> GetByStrategyId(Guid strategyId)
    {
        var premises = await _premiseService.GetByStrategyIdAsync(strategyId);
        return Ok(premises);
    }

    /// <summary>
    /// Returns the full version history of a premise, ordered from oldest to newest.
    /// </summary>
    /// <param name="id">The identifier of any premise in the version chain.</param>
    /// <response code="200">Ordered list of all versions.</response>
    /// <response code="404">Premise not found.</response>
    [HttpGet("{id:guid}/history")]
    public async Task<ActionResult<IEnumerable<PremiseResponse>>> GetVersionHistory(Guid id)
    {
        var history = await _premiseService.GetVersionHistoryAsync(id);
        return Ok(history);
    }

    /// <summary>Creates a new premise.</summary>
    /// <param name="request">The data for the new premise.</param>
    /// <response code="201">The newly created premise.</response>
    /// <response code="400">Validation failure.</response>
    [HttpPost]
    public async Task<ActionResult<PremiseResponse>> Create([FromBody] PremiseRequest request)
    {
        var created = await _premiseService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Creates a new version of an existing premise.
    /// The original premise is deactivated and a new version is created.
    /// </summary>
    /// <param name="id">The identifier of the premise to update.</param>
    /// <param name="request">The updated description.</param>
    /// <response code="200">The newly created version.</response>
    /// <response code="404">Original premise not found.</response>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PremiseResponse>> Update(Guid id, [FromBody] PremiseUpdateRequest request)
    {
        var updated = await _premiseService.UpdateAsync(id, request);
        return Ok(updated);
    }

    /// <summary>
    /// Soft-deletes a premise by deactivating it (IsActive = false).
    /// </summary>
    /// <param name="id">The identifier of the premise to deactivate.</param>
    /// <response code="204">Premise deactivated successfully.</response>
    /// <response code="404">Premise not found.</response>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        await _premiseService.DeactivateAsync(id);
        return NoContent();
    }
}
