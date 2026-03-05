namespace GQMGoalService.Application.Interfaces.Clients;

public interface IOrchestrationClient
{
    /// <summary>
    /// Records a completed step to the saga workflow. Never throws.
    /// </summary>
    Task RecordStepAsync(Guid goalId, string stepName, string compensationEndpoint, string compensationPayload);
}
