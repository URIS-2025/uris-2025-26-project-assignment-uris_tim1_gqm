namespace GoalService.Application.DTOs;

public class GoalInfluenceRequest
{
    public Guid GoalId { get; set; }
    public Guid StrategyId { get; set; }
    public string InfluenceType { get; set; } = "Positive";
    public decimal Strength { get; set; }
    public decimal Confidence { get; set; }
    public string? Notes { get; set; }
}
