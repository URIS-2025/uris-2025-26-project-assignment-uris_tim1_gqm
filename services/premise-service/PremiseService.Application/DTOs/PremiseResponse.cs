using PremiseService.Domain.Enums;

namespace PremiseService.Application.DTOs;

/// <summary>
/// Data transfer object returned from the API representing a premise.
/// </summary>
public class PremiseResponse
{
    /// <summary>Unique identifier of the premise.</summary>
    public Guid Id { get; set; }

    /// <summary>Textual description of the premise.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Type of premise: Assumption or Context.</summary>
    public PremiseType Type { get; set; }

    /// <summary>Whether this version of the premise is currently active.</summary>
    public bool IsActive { get; set; }

    /// <summary>Identifier of the previous version, or null if this is the original.</summary>
    public Guid? NewVersionOfId { get; set; }

    /// <summary>Identifier of the associated goal.</summary>
    public Guid GoalId { get; set; }

    /// <summary>Identifier of the associated strategy.</summary>
    public Guid StrategyId { get; set; }
}
