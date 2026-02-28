using Shared.Contracts;
using Microsoft.AspNetCore.Mvc;
using GQMGoalService.Application.DTOs;
using GQMGoalService.Application.DTOs.Question;
using GQMGoalService.Application.Interfaces;

namespace GQMGoalService.API.Controllers;

[ApiController]
[Route("[controller]")]
public class QuestionController : ControllerBase
{
    private readonly IQuestionService _service;

    public QuestionController(IQuestionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResponse<QuestionResponse>>> GetAll([FromQuery] PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<QuestionResponse>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("by-goal/{gqmGoalId}")]
    public async Task<ActionResult<IEnumerable<QuestionResponse>>> GetByGqmGoalId(Guid gqmGoalId, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByGqmGoalIdAsync(gqmGoalId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<QuestionResponse>> Create([FromBody] QuestionRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<QuestionResponse>> Update(Guid id, [FromBody] QuestionRequest request, CancellationToken cancellationToken = default)
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
