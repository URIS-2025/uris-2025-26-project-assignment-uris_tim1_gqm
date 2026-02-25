using GoalService.Domain.Enums;

namespace GoalService.Application.DTOs;

public record GoalRequest
{
    public string Focus { get; init; } = string.Empty;
    public string Object { get; init; } = string.Empty;
    public DateTime ActiveFrom { get; init; }
    public DateTime ActiveTo { get; init; }
    public string Magnitude { get; init; } = string.Empty;
    public string Constraints { get; init; } = string.Empty;
    public string Status { get; init; } = "Draft";
    public decimal BaselineProbability { get; init; }
    public Guid DepartmentId { get; init; }
}
