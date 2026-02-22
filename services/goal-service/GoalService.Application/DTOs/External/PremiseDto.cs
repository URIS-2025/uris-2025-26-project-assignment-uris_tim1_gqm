namespace GoalService.Application.DTOs.External;

public class PremiseDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Assumption or Context
    public bool IsActive { get; set; }
    public Guid? NewVersionOf { get; set; }
    public Guid? GoalId { get; set; }
    public Guid? StrategyId { get; set; }
}
