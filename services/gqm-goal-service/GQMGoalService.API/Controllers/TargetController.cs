using Microsoft.AspNetCore.Mvc;
using GQMGoalService.Application.DTOs.Target;
using GQMGoalService.Application.Interfaces;

namespace GQMGoalService.API.Controllers;

[ApiController]
[Route("api/v1/GQM-goal/[controller]")]
public class TargetController : ControllerBase
{
    private readonly ITargetService _service;

    public TargetController(ITargetService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TargetResponse>>> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TargetResponse>> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<TargetResponse>> Create([FromBody] TargetRequest request)
    {
        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TargetResponse>> Update(Guid id, [FromBody] TargetRequest request)
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
