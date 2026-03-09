namespace Shared.Contracts.Messages;

/// <summary>
/// Published by GoalService to trigger workflow step recording in OrchestrationService.
/// Replaces synchronous HTTP calls to IOrchestrationClient.
/// </summary>
public interface IWorkflowTransitionRequested
{
    Guid CorrelationId { get; }
    Guid GoalId { get; }
    /// <summary>
    /// "StartWorkflow" to initiate the saga, or any WorkflowStep name to record a step.
    /// </summary>
    string StepName { get; }
    string CompensationEndpoint { get; }
    string CompensationPayload { get; }
    DateTime RequestedAt { get; }
}
