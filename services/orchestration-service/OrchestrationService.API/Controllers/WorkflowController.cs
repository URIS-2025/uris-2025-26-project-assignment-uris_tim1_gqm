using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OrchestrationService.Application.DTOs;
using OrchestrationService.Application.Interfaces;

namespace OrchestrationService.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class WorkflowController : ControllerBase
{
    private readonly IWorkflowService _workflowService;
    private readonly IValidator<StartWorkflowRequest> _startValidator;
    private readonly IValidator<RecordStepRequest> _stepValidator;

    public WorkflowController(
        IWorkflowService workflowService,
        IValidator<StartWorkflowRequest> startValidator,
        IValidator<RecordStepRequest> stepValidator)
    {
        _workflowService = workflowService;
        _startValidator = startValidator;
        _stepValidator = stepValidator;
    }

    /// <summary>
    /// Start a new saga workflow.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SagaWorkflowResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> StartWorkflow([FromBody] StartWorkflowRequest request)
    {
        await _startValidator.ValidateAndThrowAsync(request);
        var result = await _workflowService.StartWorkflowAsync(request);
        return CreatedAtAction(nameof(GetWorkflow), new { goalId = result.GoalId }, result);
    }

    /// <summary>
    /// Get saga state and all steps by goalId.
    /// </summary>
    [HttpGet("{goalId:guid}")]
    [ProducesResponseType(typeof(SagaWorkflowResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkflow(Guid goalId)
    {
        var result = await _workflowService.GetWorkflowAsync(goalId);
        return Ok(result);
    }

    /// <summary>
    /// Record a completed step for this workflow.
    /// </summary>
    [HttpPost("{goalId:guid}/step")]
    [ProducesResponseType(typeof(SagaWorkflowResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RecordStep(Guid goalId, [FromBody] RecordStepRequest request)
    {
        await _stepValidator.ValidateAndThrowAsync(request);
        var result = await _workflowService.RecordStepAsync(goalId, request);
        return Ok(result);
    }

    /// <summary>
    /// Trigger compensation sequence for this workflow.
    /// </summary>
    [HttpPost("{goalId:guid}/cancel")]
    [ProducesResponseType(typeof(SagaWorkflowResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelWorkflow(Guid goalId)
    {
        var result = await _workflowService.CancelWorkflowAsync(goalId);
        return Ok(result);
    }

    /// <summary>
    /// Get full step history for a workflow.
    /// </summary>
    [HttpGet("{goalId:guid}/steps")]
    [ProducesResponseType(typeof(IEnumerable<SagaStepResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSteps(Guid goalId)
    {
        var result = await _workflowService.GetStepsAsync(goalId);
        return Ok(result);
    }
}
