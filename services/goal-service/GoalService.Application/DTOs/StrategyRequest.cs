namespace GoalService.Application.DTOs;

public record StrategyRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Effectiveness { get; init; } = "High";
    public string RefinementType { get; init; } = "OR";
    public Guid GoalId { get; init; }
}
