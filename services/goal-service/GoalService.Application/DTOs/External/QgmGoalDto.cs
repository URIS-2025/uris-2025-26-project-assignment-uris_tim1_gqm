namespace GoalService.Application.DTOs.External;

public record QgmGoalDto
{
    public Guid Id { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public Guid GoalId { get; init; }
}
