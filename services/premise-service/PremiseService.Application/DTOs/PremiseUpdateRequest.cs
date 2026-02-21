namespace PremiseService.Application.DTOs;

/// <summary>
/// Data transfer object for updating an existing premise.
/// Only the description can be changed — a new version is created automatically,
/// while Type, GoalId, and StrategyId are inherited from the original.
/// </summary>
public class PremiseUpdateRequest
{
    /// <summary>Updated textual description for the new version of the premise.</summary>
    public string Description { get; set; } = string.Empty;
}
