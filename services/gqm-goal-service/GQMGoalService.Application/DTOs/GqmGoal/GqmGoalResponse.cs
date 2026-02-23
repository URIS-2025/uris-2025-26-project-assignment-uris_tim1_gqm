using GQMGoalService.Application.DTOs.Question;

namespace GQMGoalService.Application.DTOs.GqmGoal;

public record GqmGoalResponse
{
    public Guid Id { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public Guid GoalId { get; init; }
    public ICollection<QuestionResponse> Questions { get; init; } = new List<QuestionResponse>();
}
