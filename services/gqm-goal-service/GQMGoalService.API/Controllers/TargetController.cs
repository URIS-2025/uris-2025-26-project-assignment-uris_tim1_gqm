using Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GQMGoalService.Application.DTOs;
using GQMGoalService.Application.DTOs.Target;
using GQMGoalService.Application.Interfaces;
using Shared.Auth;

namespace GQMGoalService.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class TargetController : ControllerBase
{
    private readonly ITargetService _service;

    public TargetController(ITargetService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResponse<TargetResponse>>> GetAll([FromQuery] PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TargetResponse>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("by-question/{questionId:guid}")]
    public async Task<ActionResult<IEnumerable<TargetResponse>>> GetByQuestionId(Guid questionId, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByQuestionIdAsync(questionId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [RequirePermission("create_goals")]
    public async Task<ActionResult<TargetResponse>> Create([FromBody] TargetRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("edit_goals")]
    public async Task<ActionResult<TargetResponse>> Update(Guid id, [FromBody] TargetRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("delete_goals")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
