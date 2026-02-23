namespace GQMGoalService.Application.DTOs.Measurement;

public record MeasurementResponse
{
    public Guid Id { get; init; }
    public decimal Value { get; init; }
    public DateTime MeasuredAt { get; init; }
    public Guid TargetId { get; init; }
}
