using GQMGoalService.Domain.Enums;

namespace GQMGoalService.Application.DTOs.Target;

public class TargetRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Unit Unit { get; set; }
    public Guid QuestionId { get; set; }
}
