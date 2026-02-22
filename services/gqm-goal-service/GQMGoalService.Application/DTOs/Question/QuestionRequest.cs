namespace GQMGoalService.Application.DTOs.Question;

public class QuestionRequest
{
    public string Text { get; set; } = string.Empty;
    public Guid GqmGoalId { get; set; }
}
