using PremiseService.Domain.Enums;

namespace PremiseService.Application.DTOs;

/// <summary>
/// Request record for updating (versioning) an existing premise.
/// A new version is created and the old one is deactivated.
/// </summary>
public record PremiseUpdateRequest(
    string Description,
    PremiseType Type,
    Guid? GoalId,
    Guid? StrategyId);
