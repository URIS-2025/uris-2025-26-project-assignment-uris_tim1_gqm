namespace PremiseService.Application.DTOs.External;

/// <summary>
/// External DTO representing a Strategy from the strategy-related service.
/// Used for cross-service communication when PremiseService
/// needs to fetch or validate strategy data.
/// </summary>
public record StrategyDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
