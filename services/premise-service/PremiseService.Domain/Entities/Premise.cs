using PremiseService.Domain.Enums;

namespace PremiseService.Domain.Entities;

/// <summary>
/// Represents a premise (assumption or context) associated with a goal and strategy.
/// Premises support versioning — instead of direct updates, a new version is created
/// while the previous one is deactivated (IsActive = false).
/// </summary>
public class Premise
{
    public Guid Id { get; set; }

    public string Description { get; set; } = string.Empty;

    public PremiseType Type { get; set; }

    public bool IsActive { get; set; } = true;

    public Guid? NewVersionOfId { get; set; }

    public Guid? GoalId { get; set; }

    public Guid? StrategyId { get; set; }

    // Navigation properties for version history
    public Premise? NewVersionOf { get; set; }
    public Premise? NewerVersion { get; set; }
}
