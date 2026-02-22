namespace GQMGoalService.Domain.Entities;

public class Question
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    public Guid GqmGoalId { get; set; }
    public GqmGoal GqmGoal { get; set; } = null!;
    
    public ICollection<Target> Targets { get; set; } = new List<Target>();
}
