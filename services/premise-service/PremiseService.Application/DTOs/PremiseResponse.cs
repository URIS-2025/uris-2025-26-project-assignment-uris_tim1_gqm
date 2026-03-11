using PremiseService.Domain.Enums;

namespace PremiseService.Application.DTOs;

/// <summary>
/// Full response record for a premise.
/// Used by GET /premises/{id} and POST /premises.
/// </summary>
public record PremiseResponse
{
    public Guid Id { get; init; }
    public string Description { get; init; } = string.Empty;
    public PremiseType Type { get; init; }
    public bool IsActive { get; init; }
    public Guid? NewVersionOf { get; init; }
    public Guid? GoalId { get; init; }
    public Guid? StrategyId { get; init; }
}
