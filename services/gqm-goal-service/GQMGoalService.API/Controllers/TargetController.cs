using Microsoft.AspNetCore.Mvc;
using GQMGoalService.Application.DTOs;
using GQMGoalService.Application.DTOs.Target;
using GQMGoalService.Application.Interfaces;

namespace GQMGoalService.API.Controllers;

[ApiController]
[Route("[controller]")]
public class TargetController : ControllerBase
{
    private readonly ITargetService _service;

    public TargetController(ITargetService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<TargetResponse>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(pageNumber, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TargetResponse>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("by-question/{questionId}")]
    public async Task<ActionResult<IEnumerable<TargetResponse>>> GetByQuestionId(Guid questionId, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByQuestionIdAsync(questionId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<TargetResponse>> Create([FromBody] TargetRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TargetResponse>> Update(Guid id, [FromBody] TargetRequest request, CancellationToken cancellationToken = default)
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
