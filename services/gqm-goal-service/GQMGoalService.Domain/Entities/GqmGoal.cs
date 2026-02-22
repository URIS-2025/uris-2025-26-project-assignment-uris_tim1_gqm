namespace GQMGoalService.Domain.Entities;

public class GqmGoal
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid GoalId { get; set; } // External FK to goal-service
    
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
