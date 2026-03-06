namespace OrchestrationService.Domain.Exceptions;

public class SagaNotFoundException : Exception
{
    public SagaNotFoundException(Guid goalId)
        : base($"Saga workflow for GoalId '{goalId}' was not found.") { }
}
