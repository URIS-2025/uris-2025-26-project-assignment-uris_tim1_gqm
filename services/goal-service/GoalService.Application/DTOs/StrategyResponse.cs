namespace GoalService.Application.DTOs;

public class StrategyResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Effectiveness { get; set; } = string.Empty;
    public string RefinementType { get; set; } = string.Empty;
    public Guid GoalId { get; set; }
    public List<GoalInfluenceResponse> GoalInfluences { get; set; } = new();
}
