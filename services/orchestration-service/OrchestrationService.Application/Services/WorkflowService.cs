using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrchestrationService.Application.DTOs;
using OrchestrationService.Application.Interfaces;
using OrchestrationService.Application.Interfaces.Clients;
using OrchestrationService.Application.Interfaces.Persistence;
using OrchestrationService.Domain.Entities;
using OrchestrationService.Domain.Enums;
using OrchestrationService.Domain.Exceptions;

namespace OrchestrationService.Application.Services;

public class WorkflowService : IWorkflowService
{
    private readonly IOrchestrationDbContext _context;
    private readonly IAuditClient _auditClient;
    private readonly ICompensationHttpClient _compensationClient;
    private readonly ILogger<WorkflowService> _logger;

    public WorkflowService(
        IOrchestrationDbContext context,
        IAuditClient auditClient,
        ICompensationHttpClient compensationClient,
        ILogger<WorkflowService> logger)
    {
        _context = context;
        _auditClient = auditClient;
        _compensationClient = compensationClient;
        _logger = logger;
    }

    public async Task<SagaWorkflowResponse> StartWorkflowAsync(StartWorkflowRequest request)
    {
        var existing = await _context.SagaWorkflows
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.GoalId == request.GoalId);

        if (existing is not null)
            throw new SagaAlreadyExistsException(request.GoalId);

        var now = DateTime.UtcNow;
        var workflow = new SagaWorkflow
        {
            Id = Guid.NewGuid(),
            GoalId = request.GoalId,
            Status = SagaStatus.InProgress,
            CurrentStep = WorkflowStep.GoalCreated,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.SagaWorkflows.Add(workflow);
        await _context.SaveChangesAsync();

        _ = _auditClient.LogAsync("WorkflowStarted", "SagaWorkflow", workflow.Id.ToString(),
            $"GoalId={request.GoalId}");

        return MapWorkflow(workflow);
    }

    public async Task<SagaWorkflowResponse> GetWorkflowAsync(Guid goalId)
    {
        var workflow = await _context.SagaWorkflows
            .Include(w => w.Steps)
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.GoalId == goalId)
            ?? throw new SagaNotFoundException(goalId);

        return MapWorkflow(workflow);
    }

    public async Task<SagaWorkflowResponse> RecordStepAsync(Guid goalId, RecordStepRequest request)
    {
        var workflow = await _context.SagaWorkflows
            .Include(w => w.Steps)
            .FirstOrDefaultAsync(w => w.GoalId == goalId)
            ?? throw new SagaNotFoundException(goalId);

        var step = new SagaStep
        {
            Id = Guid.NewGuid(),
            SagaWorkflowId = workflow.Id,
            StepName = request.StepName,
            Status = SagaStepStatus.Completed,
            CompensationEndpoint = request.CompensationEndpoint,
            CompensationPayload = request.CompensationPayload,
            ExecutedAt = DateTime.UtcNow
        };

        _context.SagaSteps.Add(step);

        if (Enum.TryParse<WorkflowStep>(request.StepName, ignoreCase: true, out var parsedStep))
        {
            workflow.CurrentStep = parsedStep;
            if (parsedStep == WorkflowStep.Activated)
                workflow.Status = SagaStatus.Completed;
        }

        workflow.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return MapWorkflow(workflow);
    }

    public async Task<SagaWorkflowResponse> CancelWorkflowAsync(Guid goalId)
    {
        var workflow = await _context.SagaWorkflows
            .Include(w => w.Steps)
            .FirstOrDefaultAsync(w => w.GoalId == goalId)
            ?? throw new SagaNotFoundException(goalId);

        if (workflow.Status == SagaStatus.Compensated)
            throw new SagaAlreadyCompensatedException(goalId);

        workflow.Status = SagaStatus.Compensating;
        workflow.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var completedSteps = workflow.Steps
            .Where(s => s.Status == SagaStepStatus.Completed)
            .OrderByDescending(s => s.ExecutedAt)
            .ToList();

        foreach (var step in completedSteps)
        {
            var success = await _compensationClient.CallAsync(step.CompensationEndpoint, step.CompensationPayload);

            if (!success)
            {
                _logger.LogWarning(
                    "Compensation call failed for step '{StepName}' (endpoint: {Endpoint}). Continuing.",
                    step.StepName, step.CompensationEndpoint);
            }

            step.Status = SagaStepStatus.Compensated;
            step.CompensatedAt = DateTime.UtcNow;
        }

        workflow.Status = SagaStatus.Compensated;
        workflow.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _ = _auditClient.LogAsync("WorkflowCompensated", "SagaWorkflow", workflow.Id.ToString(),
            $"GoalId={goalId}");

        return MapWorkflow(workflow);
    }

    public async Task<IEnumerable<SagaStepResponse>> GetStepsAsync(Guid goalId)
    {
        var workflow = await _context.SagaWorkflows
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.GoalId == goalId)
            ?? throw new SagaNotFoundException(goalId);

        var steps = await _context.SagaSteps
            .AsNoTracking()
            .Where(s => s.SagaWorkflowId == workflow.Id)
            .OrderBy(s => s.ExecutedAt)
            .ToListAsync();

        return steps.Select(MapStep);
    }

    // --- Mapping helpers ---

    private static SagaWorkflowResponse MapWorkflow(SagaWorkflow w) => new()
    {
        Id = w.Id,
        GoalId = w.GoalId,
        Status = w.Status.ToString(),
        CurrentStep = w.CurrentStep.ToString(),
        CreatedAt = w.CreatedAt,
        UpdatedAt = w.UpdatedAt,
        Steps = w.Steps.OrderBy(s => s.ExecutedAt).Select(MapStep).ToList()
    };

    private static SagaStepResponse MapStep(SagaStep s) => new()
    {
        Id = s.Id,
        SagaWorkflowId = s.SagaWorkflowId,
        StepName = s.StepName,
        Status = s.Status.ToString(),
        CompensationEndpoint = s.CompensationEndpoint,
        CompensationPayload = s.CompensationPayload,
        ExecutedAt = s.ExecutedAt,
        CompensatedAt = s.CompensatedAt
    };
}
