using PremiseService.Domain.Enums;

namespace PremiseService.Application.DTOs;

/// <summary>
/// Data transfer object for creating a new premise.
/// </summary>
public class PremiseRequest
{
    /// <summary>Textual description of the premise.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Type of premise: Assumption or Context.</summary>
    public PremiseType Type { get; set; }

    /// <summary>Identifier of the goal this premise is associated with.</summary>
    public Guid GoalId { get; set; }

    /// <summary>Identifier of the strategy this premise is associated with.</summary>
    public Guid StrategyId { get; set; }
}
