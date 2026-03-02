using AssessmentService.Domain.Enums;

namespace AssessmentService.Domain.Entities;

public class GoalProbabilityAssessment
{
    public Guid Id { get; set; }
    public Guid GoalId { get; set; }
    public decimal Probability { get; set; }
    public AssessmentState State { get; set; }
    public AssessmentMethod Method { get; set; }
    public string Notes { get; set; } = string.Empty;
}
