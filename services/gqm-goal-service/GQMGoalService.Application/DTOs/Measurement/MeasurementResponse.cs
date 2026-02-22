namespace GQMGoalService.Application.DTOs.Measurement;

public class MeasurementResponse
{
    public Guid Id { get; set; }
    public decimal Value { get; set; }
    public DateTime MeasuredAt { get; set; }
    public Guid TargetId { get; set; }
}
