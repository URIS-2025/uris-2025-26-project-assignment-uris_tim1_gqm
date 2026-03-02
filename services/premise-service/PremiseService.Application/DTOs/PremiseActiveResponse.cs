using PremiseService.Domain.Enums;

namespace PremiseService.Application.DTOs;

/// <summary>
/// Minimal response record for active premise queries (by goal or strategy).
/// </summary>
public record PremiseActiveResponse
{
    public Guid Id { get; init; }
    public string Description { get; init; } = string.Empty;
    public PremiseType Type { get; init; }
    public bool IsActive { get; init; }
}
