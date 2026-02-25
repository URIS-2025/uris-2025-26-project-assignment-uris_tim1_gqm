namespace GoalService.Application.DTOs.External;

public record PremiseDto
{
    public Guid Id { get; init; }
    public string Description { get; init; } = string.Empty;
    public string Type { get; init; } // Assumption or Context
    public bool IsActive { get; init; }
    public Guid? NewVersionOf { get; init; }
    public Guid? GoalId { get; init; }
    public Guid? StrategyId { get; init; }
}
