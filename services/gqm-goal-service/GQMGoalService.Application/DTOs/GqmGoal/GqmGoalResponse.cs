using GQMGoalService.Application.DTOs.Question;

namespace GQMGoalService.Application.DTOs.GqmGoal;

public class GqmGoalResponse
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid GoalId { get; set; }
    public ICollection<QuestionResponse> Questions { get; set; } = new List<QuestionResponse>();
}
