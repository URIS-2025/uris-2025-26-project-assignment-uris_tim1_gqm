namespace GoalService.Application.DTOs;

/// <summary>
/// Recursive DTO for goal tree visualization.
/// Represents a goal node with its strategies (which may contain child goals).
/// </summary>
public record GoalTreeNodeResponse
{
    public Guid Id { get; init; }
    public string Focus { get; init; } = string.Empty;
    public string Object { get; init; } = string.Empty;
    public string Status { get; init; } = "Draft";
    public decimal BaselineProbability { get; init; }
    public Guid DepartmentId { get; init; }
    public DateTime ActiveFrom { get; init; }
    public DateTime ActiveTo { get; init; }
    public string Magnitude { get; init; } = string.Empty;
    public string Constraints { get; init; } = string.Empty;
    public List<StrategyTreeNodeResponse> Strategies { get; init; } = [];
}

/// <summary>
/// Strategy node in the goal tree.
/// Contains child goals produced by this strategy via GoalInfluence.
/// </summary>
public record StrategyTreeNodeResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string RefinementType { get; init; } = "OR";
    public string Effectiveness { get; init; } = "Medium";
    public bool IsActive { get; init; }
    public List<ChildGoalInfluenceResponse> ChildGoals { get; init; } = [];
}

/// <summary>
/// Child goal with influence metadata from GoalInfluence entity.
/// </summary>
public record ChildGoalInfluenceResponse
{
    public GoalTreeNodeResponse Goal { get; init; } = null!;
    public string InfluenceType { get; init; } = "Positive";
    public decimal Strength { get; init; }
    public decimal Confidence { get; init; }
    public string? Notes { get; init; }
}
