namespace GoalService.Application.DTOs.External;

public class AssessmentDto
{
    public Guid Id { get; set; }
    public Guid GoalId { get; set; }
    public decimal Probability { get; set; }
    public string State { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
