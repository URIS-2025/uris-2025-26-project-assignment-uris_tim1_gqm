namespace GQMGoalService.Application.DTOs.Measurement;

public record MeasurementRequest
{
    public decimal Value { get; init; }
    public DateTime? MeasuredAt { get; init; }
    public Guid TargetId { get; init; }
}
