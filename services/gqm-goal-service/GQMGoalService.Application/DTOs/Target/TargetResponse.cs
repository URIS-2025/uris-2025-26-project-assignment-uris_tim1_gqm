using GQMGoalService.Application.DTOs.Measurement;
using GQMGoalService.Domain.Enums;

namespace GQMGoalService.Application.DTOs.Target;

public record TargetResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Unit Unit { get; set; }
    public Guid QuestionId { get; set; }
    public ICollection<MeasurementResponse> Measurements { get; set; } = new List<MeasurementResponse>();
}
