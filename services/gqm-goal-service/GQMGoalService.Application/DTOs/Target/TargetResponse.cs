using GQMGoalService.Application.DTOs.Measurement;
using GQMGoalService.Domain.Enums;

namespace GQMGoalService.Application.DTOs.Target;

public record TargetResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Unit Unit { get; init; }
    public Guid QuestionId { get; init; }
    public ICollection<MeasurementResponse> Measurements { get; init; } = new List<MeasurementResponse>();
}
