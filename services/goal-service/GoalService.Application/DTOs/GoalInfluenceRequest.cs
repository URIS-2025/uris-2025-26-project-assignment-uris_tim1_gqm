namespace GoalService.Application.DTOs;

public record GoalInfluenceRequest
{
    public Guid GoalId { get; init; }
    public Guid StrategyId { get; init; }
    public string InfluenceType { get; init; }
    public decimal Strength { get; init; }
    public decimal Confidence { get; init; }
    public string? Notes { get; init; }
}
