namespace GoalService.Application.DTOs;

public class GoalInfluenceResponse
{
    public Guid GoalId { get; set; }
    public Guid StrategyId { get; set; }
    public string InfluenceType { get; set; } = string.Empty;
    public decimal Strength { get; set; }
    public decimal Confidence { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Notes { get; set; }
}
