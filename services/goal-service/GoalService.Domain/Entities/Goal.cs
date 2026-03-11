using GoalService.Domain.Enums;

namespace GoalService.Domain.Entities;

public class Goal
{
    public Guid Id { get; set; }

    /// <summary>
    /// What the goal focuses on (e.g., "Develop the marketability").
    /// </summary>
    public string Focus { get; set; } = string.Empty;

    /// <summary>
    /// The object of the goal (e.g., "for IP testing products").
    /// </summary>
    public string Object { get; set; } = string.Empty;

    /// <summary>
    /// Start of the goal's active timeframe.
    /// </summary>
    public DateTime ActiveFrom { get; set; }

    /// <summary>
    /// End of the goal's active timeframe.
    /// </summary>
    public DateTime ActiveTo { get; set; }

    /// <summary>
    /// Scope/magnitude of the goal (e.g., "50% coverage of customer needs").
    /// </summary>
    public string Magnitude { get; set; } = string.Empty;

    /// <summary>
    /// Constraints of the goal (e.g., "resources, IP competence, compete with existing competitors").
    /// </summary>
    public string Constraints { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the goal lifecycle.
    /// </summary>
    public GoalStatus Status { get; set; } = GoalStatus.Draft;

    /// <summary>
    /// Baseline probability of achieving the goal (0.0 - 1.0).
    /// </summary>
    public decimal BaselineProbability { get; set; }

    /// <summary>
    /// Reference to the Department that owns this goal (cross-service, by ID only).
    /// </summary>
    public Guid DepartmentId { get; set; }

    // --- Navigation Properties (within aggregate) ---

    /// <summary>
    /// Strategies defined to achieve this goal.
    /// </summary>
    public ICollection<Strategy> Strategies { get; set; } = new List<Strategy>();

    /// <summary>
    /// If this goal arose from a strategy, this is the influence record.
    /// A goal can arise from at most one strategy (0..1 relationship).
    /// </summary>
    public GoalInfluence? GoalInfluence { get; set; }
}
