using GQMGoalService.Application.DTOs.Target;

namespace GQMGoalService.Application.DTOs.Question;

public record QuestionResponse
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid GqmGoalId { get; set; }
    public ICollection<TargetResponse> Targets { get; set; } = new List<TargetResponse>();
}
