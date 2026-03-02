using GQMGoalService.Domain.Enums;

namespace GQMGoalService.Domain.Entities;

public class Target
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Unit Unit { get; set; }
    
    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    
    public ICollection<Measurement> Measurements { get; set; } = new List<Measurement>();
}
