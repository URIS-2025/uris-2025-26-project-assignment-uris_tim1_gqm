namespace OrchestrationService.Application.DTOs;

public class SagaStepResponse
{
    public Guid Id { get; set; }
    public Guid SagaWorkflowId { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CompensationEndpoint { get; set; } = string.Empty;
    public string CompensationPayload { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; }
    public DateTime? CompensatedAt { get; set; }
}
