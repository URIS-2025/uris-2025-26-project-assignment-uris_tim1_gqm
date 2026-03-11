using MassTransit;
using Microsoft.Extensions.Logging;
using OrchestrationService.Application.DTOs;
using OrchestrationService.Application.Interfaces;
using Shared.Contracts.Messages;
using System;
using System.Threading.Tasks;

namespace OrchestrationService.Infrastructure.Consumers;

public class WorkflowTransitionRequestedConsumer : IConsumer<IWorkflowTransitionRequested>
{
    private readonly IWorkflowService _workflowService;
    private readonly ILogger<WorkflowTransitionRequestedConsumer> _logger;

    public WorkflowTransitionRequestedConsumer(IWorkflowService workflowService, ILogger<WorkflowTransitionRequestedConsumer> logger)
    {
        _workflowService = workflowService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IWorkflowTransitionRequested> context)
    {
        var msg = context.Message;
        
        try
        {
            if (msg.StepName.Equals("StartWorkflow", StringComparison.OrdinalIgnoreCase))
            {
                var request = new StartWorkflowRequest { GoalId = msg.GoalId };
                await _workflowService.StartWorkflowAsync(request);
                _logger.LogInformation("Started workflow for GoalId: {GoalId} on behalf of consumer", msg.GoalId);
            }
            else
            {
                var request = new RecordStepRequest
                {
                    StepName = msg.StepName,
                    CompensationEndpoint = msg.CompensationEndpoint,
                    CompensationPayload = msg.CompensationPayload
                };
                await _workflowService.RecordStepAsync(msg.GoalId, request);
                _logger.LogInformation("Recorded step '{StepName}' for GoalId: {GoalId} on behalf of consumer", msg.StepName, msg.GoalId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process WorkflowTransitionRequested for GoalId: {GoalId}, StepName: {StepName}", msg.GoalId, msg.StepName);
            // Re-throw so MassTransit retry/fault mechanisms take over
            throw;
        }
    }
}
