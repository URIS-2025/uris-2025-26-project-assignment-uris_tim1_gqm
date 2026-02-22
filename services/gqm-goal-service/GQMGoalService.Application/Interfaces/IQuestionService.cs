using GQMGoalService.Application.DTOs.Question;

namespace GQMGoalService.Application.Interfaces;

public interface IQuestionService
{
    Task<IEnumerable<QuestionResponse>> GetAllAsync();
    Task<QuestionResponse> GetByIdAsync(Guid id);
    Task<QuestionResponse> CreateAsync(QuestionRequest request);
    Task<QuestionResponse> UpdateAsync(Guid id, QuestionRequest request);
    Task<bool> DeleteAsync(Guid id);
}
