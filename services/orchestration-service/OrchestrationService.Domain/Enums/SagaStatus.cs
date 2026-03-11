namespace OrchestrationService.Domain.Enums;

public enum SagaStatus
{
    InProgress,
    Completed,
    Compensating,
    Compensated,
    Failed
}
