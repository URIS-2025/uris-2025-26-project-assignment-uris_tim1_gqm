namespace OrchestrationService.Domain.Exceptions;

public class SagaAlreadyCompensatedException : Exception
{
    public SagaAlreadyCompensatedException(Guid goalId)
        : base($"Saga workflow for GoalId '{goalId}' has already been compensated.") { }
}
