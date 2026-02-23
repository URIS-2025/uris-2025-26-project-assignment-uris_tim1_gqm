namespace GoalService.Application.DTOs;

public record StrategyRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Effectiveness { get; set; } = "Medium";
    public string RefinementType { get; set; } = "AND";
    public Guid GoalId { get; set; }
}
