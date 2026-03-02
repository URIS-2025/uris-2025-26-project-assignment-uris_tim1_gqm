namespace PremiseService.Application.DTOs.External;

/// <summary>
/// External DTO representing a Goal from GoalService.
/// Used for cross-service communication when PremiseService
/// needs to fetch or validate goal data.
/// </summary>
public record GoalDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
