namespace OrchestrationService.Application.DTOs;

public class SagaWorkflowResponse
{
    public Guid Id { get; set; }
    public Guid GoalId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CurrentStep { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<SagaStepResponse> Steps { get; set; } = new();
}
