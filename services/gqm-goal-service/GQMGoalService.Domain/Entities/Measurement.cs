namespace GQMGoalService.Domain.Entities;

public class Measurement
{
    public Guid Id { get; set; }
    public decimal Value { get; set; }
    public DateTime MeasuredAt { get; set; }
    
    public Guid TargetId { get; set; }
    public Target Target { get; set; } = null!;
}
