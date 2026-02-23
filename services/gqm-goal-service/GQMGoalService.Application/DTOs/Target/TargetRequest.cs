using GQMGoalService.Domain.Enums;

namespace GQMGoalService.Application.DTOs.Target;

public record TargetRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Unit Unit { get; init; }
    public Guid QuestionId { get; init; }
}
