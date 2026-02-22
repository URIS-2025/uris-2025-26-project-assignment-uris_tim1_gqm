namespace GoalService.Application.DTOs.External;

public class QgmGoalDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid GoalId { get; set; }
}
