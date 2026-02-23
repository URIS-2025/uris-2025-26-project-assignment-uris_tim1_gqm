using GQMGoalService.Application.DTOs;
using GQMGoalService.Application.DTOs.Question;

namespace GQMGoalService.Application.Interfaces;

public interface IQuestionService
{
    Task<PagedResult<QuestionResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<QuestionResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<QuestionResponse>> GetByGqmGoalIdAsync(Guid gqmGoalId, CancellationToken cancellationToken = default);
    Task<QuestionResponse> CreateAsync(QuestionRequest request, CancellationToken cancellationToken = default);
    Task<QuestionResponse> UpdateAsync(Guid id, QuestionRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
