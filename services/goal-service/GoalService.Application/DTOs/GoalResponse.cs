namespace GoalService.Application.DTOs;

public record GoalResponse
{
    public Guid Id { get; init; }
    public string Focus { get; init; } = string.Empty;
    public string Object { get; init; } = string.Empty;
    public DateTime ActiveFrom { get; init; }
    public DateTime ActiveTo { get; init; }
    public string Magnitude { get; init; } = string.Empty;
    public string Constraints { get; init; } = string.Empty;
    public string Status { get; init; }
    public decimal BaselineProbability { get; init; }
    public Guid DepartmentId { get; init; }
    public List<StrategyResponse> Strategies { get; init; } = new();
    public GoalInfluenceResponse? GoalInfluence { get; init; }
}
