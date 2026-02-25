namespace GoalService.Application.DTOs;

public record StrategyResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Effectiveness { get; init; }
    public string RefinementType { get; init; }
    public Guid GoalId { get; init; }
    public List<GoalInfluenceResponse> GoalInfluences { get; set; } = new();
}
