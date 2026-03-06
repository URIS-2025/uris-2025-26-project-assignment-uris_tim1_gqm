using OrchestrationService.Domain.Enums;

namespace OrchestrationService.Domain.Entities;

public class SagaWorkflow
{
    public Guid Id { get; set; }
    public Guid GoalId { get; set; }
    public SagaStatus Status { get; set; } = SagaStatus.InProgress;
    public WorkflowStep CurrentStep { get; set; } = WorkflowStep.GoalCreated;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<SagaStep> Steps { get; set; } = new List<SagaStep>();
}
