using Microsoft.AspNetCore.Mvc;
using PremiseService.Application.DTOs;
using PremiseService.Application.Interfaces;

namespace PremiseService.API.Controllers;

/// <summary>
/// REST API controller for managing premises within the GQM+ Strategy model.
/// </summary>
[ApiController]
[Route("premises")]
public class PremiseController : ControllerBase
{
    private readonly IPremiseService _premiseService;

    public PremiseController(IPremiseService premiseService)
    {
        _premiseService = premiseService;
    }

    /// <summary>List premises with pagination.</summary>
    /// <response code="200">Paginated list of premises.</response>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<PremiseResponse>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var result = await _premiseService.GetAllAsync(page, size);
        return Ok(result);
    }

    /// <summary>Get a single premise by ID.</summary>
    /// <response code="200">The requested premise.</response>
    /// <response code="404">Premise not found.</response>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PremiseResponse>> GetById(Guid id)
    {
        var premise = await _premiseService.GetByIdAsync(id);
        return Ok(premise);
    }

    /// <summary>Get active premises by goal.</summary>
    /// <response code="200">List of active premises for the given goal.</response>
    [HttpGet("active/goal/{goalId:guid}")]
    public async Task<ActionResult<IEnumerable<PremiseActiveResponse>>> GetActiveByGoal(Guid goalId)
    {
        var premises = await _premiseService.GetActiveByGoalIdAsync(goalId);
        return Ok(premises);
    }

    /// <summary>Get active premises by strategy.</summary>
    /// <response code="200">List of active premises for the given strategy.</response>
    [HttpGet("active/strategy/{strategyId:guid}")]
    public async Task<ActionResult<IEnumerable<PremiseActiveResponse>>> GetActiveByStrategy(Guid strategyId)
    {
        var premises = await _premiseService.GetActiveByStrategyIdAsync(strategyId);
        return Ok(premises);
    }

    /// <summary>Create a new premise.</summary>
    /// <response code="201">The newly created premise.</response>
    /// <response code="400">Validation failure.</response>
    [HttpPost]
    public async Task<ActionResult<PremiseResponse>> Create([FromBody] PremiseRequest request)
    {
        var created = await _premiseService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Delete a premise.</summary>
    /// <response code="204">Premise deleted successfully.</response>
    /// <response code="404">Premise not found.</response>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _premiseService.DeleteAsync(id);
        return NoContent();
    }
}
