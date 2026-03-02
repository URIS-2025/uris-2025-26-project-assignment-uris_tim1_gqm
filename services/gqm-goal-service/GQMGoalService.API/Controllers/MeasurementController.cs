using Shared.Contracts;
using Microsoft.AspNetCore.Mvc;
using GQMGoalService.Application.DTOs;
using GQMGoalService.Application.DTOs.Measurement;
using GQMGoalService.Application.Interfaces;

namespace GQMGoalService.API.Controllers;

[ApiController]
[Route("[controller]")]
public class MeasurementController : ControllerBase
{
    private readonly IMeasurementService _service;

    public MeasurementController(IMeasurementService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResponse<MeasurementResponse>>> GetAll([FromQuery] PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MeasurementResponse>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("by-target/{targetId}")]
    public async Task<ActionResult<IEnumerable<MeasurementResponse>>> GetByTargetId(Guid targetId, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByTargetIdAsync(targetId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<MeasurementResponse>> Create([FromBody] MeasurementRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<MeasurementResponse>> Update(Guid id, [FromBody] MeasurementRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
