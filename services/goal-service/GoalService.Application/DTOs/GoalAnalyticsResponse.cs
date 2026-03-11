namespace GoalService.Application.DTOs;

/// <summary>
/// Analytics data for goals within a scope (department or root goal tree).
/// </summary>
public record GoalAnalyticsResponse
{
    // KPI counts
    public int TotalGoals { get; init; }
    public int ActiveGoals { get; init; }
    public int CompletedGoals { get; init; }
    public int DraftGoals { get; init; }
    
    // Status distribution for donut chart
    public Dictionary<string, int> StatusDistribution { get; init; } = new();
    
    // Probability distribution for bar chart (5 buckets: 0-20%, 20-40%, 40-60%, 60-80%, 80-100%)
    public Dictionary<string, int> ProbabilityDistribution { get; init; } = new();
    
    // Depth distribution (level 0 = root, level 1 = first derived, etc.)
    public Dictionary<int, int> DepthDistribution { get; init; } = new();
    
    // Strategy refinement type distribution (AND vs OR)
    public Dictionary<string, int> RefinementDistribution { get; init; } = new();
    
    // Auto-generated insights
    public GoalInsightResponse? HighestProbabilityGoal { get; init; }
    public GoalInsightResponse? LowestProbabilityActiveGoal { get; init; }
    public StrategyInsightResponse? MostProductiveStrategy { get; init; }
    public DepartmentInsightResponse? MostActiveDepartment { get; init; }
}

/// <summary>
/// Goal insight for analytics highlights.
/// </summary>
public record GoalInsightResponse
{
    public Guid Id { get; init; }
    public string Focus { get; init; } = string.Empty;
    public string Object { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal BaselineProbability { get; init; }
    public Guid DepartmentId { get; init; }
}

/// <summary>
/// Strategy insight for analytics highlights.
/// </summary>
public record StrategyInsightResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid GoalId { get; init; }
    public string GoalFocus { get; init; } = string.Empty;
    public int DerivedGoalsCount { get; init; }
}

/// <summary>
/// Department insight for organization-wide analytics.
/// </summary>
public record DepartmentInsightResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int ActiveGoalsCount { get; init; }
}
