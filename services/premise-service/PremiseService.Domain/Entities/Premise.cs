using PremiseService.Domain.Enums;

namespace PremiseService.Domain.Entities;

/// <summary>
/// Represents a premise (assumption or context) associated with a goal and strategy.
/// Premises support versioning — instead of direct updates, a new version is created
/// while the previous one is deactivated (IsActive = false).
/// </summary>
public class Premise
{
    /// <summary>Unique identifier of the premise.</summary>
    public Guid Id { get; set; }

    /// <summary>Textual description of the premise.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Type of premise: Assumption or Context.</summary>
    public PremiseType Type { get; set; }

    /// <summary>Indicates whether this premise version is currently active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// References the previous version of this premise (self-referencing FK).
    /// Null if this is the original version.
    /// </summary>
    public Guid? NewVersionOfId { get; set; }

    /// <summary>
    /// Identifier of the goal this premise belongs to.
    /// Stored as a plain GUID — no FK constraint, as Goal lives in a separate microservice.
    /// </summary>
    public Guid GoalId { get; set; }

    /// <summary>
    /// Identifier of the strategy this premise belongs to.
    /// Stored as a plain GUID — no FK constraint, as Strategy lives in a separate microservice.
    /// </summary>
    public Guid StrategyId { get; set; }

    /// <summary>Navigation property to the previous version of this premise.</summary>
    public Premise? NewVersionOf { get; set; }

    /// <summary>Navigation property to the newer version that replaced this premise.</summary>
    public Premise? NewerVersion { get; set; }
}
