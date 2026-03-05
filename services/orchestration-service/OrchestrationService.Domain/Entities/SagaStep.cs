using OrchestrationService.Domain.Enums;

namespace OrchestrationService.Domain.Entities;

public class SagaStep
{
    public Guid Id { get; set; }
    public Guid SagaWorkflowId { get; set; }
    public string StepName { get; set; } = string.Empty;
    public SagaStepStatus Status { get; set; } = SagaStepStatus.Pending;
    public string CompensationEndpoint { get; set; } = string.Empty;
    public string CompensationPayload { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; }
    public DateTime? CompensatedAt { get; set; }

    public SagaWorkflow Workflow { get; set; } = null!;
}
