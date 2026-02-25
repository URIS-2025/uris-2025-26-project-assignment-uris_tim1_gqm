using GoalService.Domain.Enums;

namespace GoalService.Domain.Entities;

public class Strategy
{
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the strategy (e.g., "Use MoSCoW method to prioritize development efforts").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the strategy.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Assessed effectiveness level of this strategy.
    /// </summary>
    public EffectivenessLevel Effectiveness { get; set; }

    /// <summary>
    /// Defines how child goals combine toward achieving the parent goal.
    /// AND = all child goals must be achieved; OR = at least one is sufficient.
    /// </summary>
    public RefinementType RefinementType { get; set; }

    /// <summary>
    /// The goal this strategy belongs to.
    /// </summary>
    public Guid GoalId { get; set; }

    // --- Navigation Properties ---

    /// <summary>
    /// The parent goal this strategy is defined for.
    /// </summary>
    public Goal Goal { get; set; } = null!;

    /// <summary>
    /// Goals that arose from this strategy (modeled via GoalInfluence).
    /// A strategy can produce multiple child goals.
    /// </summary>
    public ICollection<GoalInfluence> GoalInfluences { get; set; } = new List<GoalInfluence>();
}
