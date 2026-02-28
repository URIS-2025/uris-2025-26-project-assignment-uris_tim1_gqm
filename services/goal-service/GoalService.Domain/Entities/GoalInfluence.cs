using GoalService.Domain.Enums;

namespace GoalService.Domain.Entities;

/// <summary>
/// Models the relationship between a goal that arose from a strategy.
/// This is the key entity for building the goal hierarchy (tree structure).
/// 
/// Relationships:
///   - 0/1 : 1 to Goal  — a goal can have at most one GoalInfluence (arose from at most one strategy)
///   - N : 1 to Strategy — a strategy can produce multiple child goals
///
/// Constraint: The goal hierarchy must form a tree (no cycles allowed).
/// There must always be exactly one path from any ancestor goal to any descendant goal.
/// </summary>
public class GoalInfluence
{
    /// <summary>
    /// The goal that arose from the strategy.
    /// Also serves as the primary key (one GoalInfluence per goal).
    /// </summary>
    public Guid GoalId { get; set; }

    /// <summary>
    /// The strategy from which this goal arose.
    /// </summary>
    public Guid StrategyId { get; set; }

    /// <summary>
    /// Type of influence (positive, negative, or neutral).
    /// </summary>
    public InfluenceType InfluenceType { get; set; }

    /// <summary>
    /// Strength of the influence (0.0 - 1.0).
    /// </summary>
    public decimal Strength { get; set; }

    /// <summary>
    /// Confidence level in the influence assessment (0.0 - 1.0).
    /// </summary>
    public decimal Confidence { get; set; }

    /// <summary>
    /// Timestamp when this influence relationship was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Optional notes about the influence relationship.
    /// </summary>
    public string? Notes { get; set; }

    // --- Navigation Properties ---

    /// <summary>
    /// The goal that arose from the strategy.
    /// </summary>
    public Goal Goal { get; set; } = null!;

    /// <summary>
    /// The strategy that produced this goal.
    /// </summary>
    public Strategy Strategy { get; set; } = null!;
}
