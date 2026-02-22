using Microsoft.AspNetCore.Mvc;
using GQMGoalService.Application.DTOs.Measurement;
using GQMGoalService.Application.Interfaces;

namespace GQMGoalService.API.Controllers;

[ApiController]
[Route("api/v1/GQM-goal/[controller]")]
public class MeasurementController : ControllerBase
{
    private readonly IMeasurementService _service;

    public MeasurementController(IMeasurementService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MeasurementResponse>>> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MeasurementResponse>> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<MeasurementResponse>> Create([FromBody] MeasurementRequest request)
    {
        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<MeasurementResponse>> Update(Guid id, [FromBody] MeasurementRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
