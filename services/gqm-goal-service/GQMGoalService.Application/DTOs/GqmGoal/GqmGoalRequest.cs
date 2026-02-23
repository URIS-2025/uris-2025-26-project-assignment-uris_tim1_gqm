namespace GQMGoalService.Application.DTOs.GqmGoal;

public record GqmGoalRequest
{
    public string Description { get; init; } = string.Empty;
    public Guid GoalId { get; init; }
}
