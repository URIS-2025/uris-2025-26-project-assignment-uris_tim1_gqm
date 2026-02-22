namespace GoalService.Application.DTOs;

public class GoalResponse
{
    public Guid Id { get; set; }
    public string Focus { get; set; } = string.Empty;
    public string Object { get; set; } = string.Empty;
    public DateTime ActiveFrom { get; set; }
    public DateTime ActiveTo { get; set; }
    public string Magnitude { get; set; } = string.Empty;
    public string Constraints { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal BaselineProbability { get; set; }
    public Guid DepartmentId { get; set; }
    public List<StrategyResponse> Strategies { get; set; } = new();
    public GoalInfluenceResponse? GoalInfluence { get; set; }
}
