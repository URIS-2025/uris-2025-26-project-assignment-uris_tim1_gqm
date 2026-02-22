namespace GQMGoalService.Application.DTOs.Measurement;

public record MeasurementRequest
{
    public decimal Value { get; set; }
    public DateTime? MeasuredAt { get; set; }
    public Guid TargetId { get; set; }
}
