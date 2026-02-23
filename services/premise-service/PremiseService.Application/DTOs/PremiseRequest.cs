using PremiseService.Domain.Enums;

namespace PremiseService.Application.DTOs;

/// <summary>
/// Request record for creating a new premise.
/// </summary>
public record PremiseRequest(
    string Description,
    PremiseType Type,
    Guid GoalId,
    Guid StrategyId);
