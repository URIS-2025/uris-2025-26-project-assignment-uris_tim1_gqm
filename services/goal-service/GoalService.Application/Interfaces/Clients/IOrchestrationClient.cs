namespace GoalService.Application.Interfaces.Clients;

public interface IOrchestrationClient
{
    /// <summary>
    /// Starts a new saga workflow for the given goal. Never throws.
    /// </summary>
    Task StartWorkflowAsync(Guid goalId);

    /// <summary>
    /// Records a completed step to the saga workflow. Never throws.
    /// </summary>
    Task RecordStepAsync(Guid goalId, string stepName, string compensationEndpoint, string compensationPayload);
}
