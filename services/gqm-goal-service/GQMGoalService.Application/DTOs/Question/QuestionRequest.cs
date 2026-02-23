namespace GQMGoalService.Application.DTOs.Question;

public record QuestionRequest
{
    public string Text { get; init; } = string.Empty;
    public Guid GqmGoalId { get; init; }
}
