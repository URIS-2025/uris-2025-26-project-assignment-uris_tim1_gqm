namespace OrchestrationService.Domain.Exceptions;

public class SagaAlreadyExistsException : Exception
{
    public SagaAlreadyExistsException(Guid goalId)
        : base($"A saga workflow for GoalId '{goalId}' already exists.") { }
}
