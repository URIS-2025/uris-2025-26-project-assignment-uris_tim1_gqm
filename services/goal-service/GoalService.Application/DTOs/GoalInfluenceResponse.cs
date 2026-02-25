namespace GoalService.Application.DTOs;

public record GoalInfluenceResponse
{
    public Guid GoalId { get; init; }
    public Guid StrategyId { get; init; }
    public string InfluenceType { get; init; } = "Positive";
    public decimal Strength { get; init; }
    public decimal Confidence { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? Notes { get; init; }
}
