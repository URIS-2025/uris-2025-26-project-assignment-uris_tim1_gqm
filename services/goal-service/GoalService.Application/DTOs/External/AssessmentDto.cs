namespace GoalService.Application.DTOs.External;

public record AssessmentDto
{
    public Guid Id { get; init; }
    public Guid GoalId { get; init; }
    public decimal Probability { get; init; }
    public string State { get; init; } = string.Empty;
    public string Method { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
}
