namespace PremiseService.Domain.Enums;

/// <summary>
/// Defines the type of a premise within the GQM+ Strategy model.
/// </summary>
public enum PremiseType
{
    /// <summary>An assumption that is believed to be true but not yet verified.</summary>
    Assumption,

    /// <summary>A contextual fact or condition relevant to the goal or strategy.</summary>
    Context
}
