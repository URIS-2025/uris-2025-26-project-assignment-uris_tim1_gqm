namespace GQMGoalService.Application.DTOs.GqmGoal;

public class GqmGoalRequest
{
    public string Description { get; set; } = string.Empty;
    public Guid GoalId { get; set; }
}
