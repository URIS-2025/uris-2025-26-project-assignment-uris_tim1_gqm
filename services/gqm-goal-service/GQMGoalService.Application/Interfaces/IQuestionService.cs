using GQMGoalService.Application.DTOs;
using GQMGoalService.Application.DTOs.Question;

namespace GQMGoalService.Application.Interfaces;

public interface IQuestionService
{
    Task<PagedResult<QuestionResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
    Task<QuestionResponse> GetByIdAsync(Guid id);
    Task<IEnumerable<QuestionResponse>> GetByGqmGoalIdAsync(Guid gqmGoalId);
    Task<QuestionResponse> CreateAsync(QuestionRequest request);
    Task<QuestionResponse> UpdateAsync(Guid id, QuestionRequest request);
    Task<bool> DeleteAsync(Guid id);
}
