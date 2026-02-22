namespace GQMGoalService.Application.DTOs.Measurement;

public class MeasurementRequest
{
    public decimal Value { get; set; }
    public DateTime? MeasuredAt { get; set; }
    public Guid TargetId { get; set; }
}
