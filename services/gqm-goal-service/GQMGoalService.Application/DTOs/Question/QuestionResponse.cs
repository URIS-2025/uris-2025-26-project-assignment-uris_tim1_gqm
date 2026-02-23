using GQMGoalService.Application.DTOs.Target;

namespace GQMGoalService.Application.DTOs.Question;

public record QuestionResponse
{
    public Guid Id { get; init; }
    public string Text { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public Guid GqmGoalId { get; init; }
    public ICollection<TargetResponse> Targets { get; init; } = new List<TargetResponse>();
}
