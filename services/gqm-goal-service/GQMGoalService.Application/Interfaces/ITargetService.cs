using GQMGoalService.Application.DTOs;
using GQMGoalService.Application.DTOs.Target;

namespace GQMGoalService.Application.Interfaces;

public interface ITargetService
{
    Task<PagedResult<TargetResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
    Task<TargetResponse> GetByIdAsync(Guid id);
    Task<IEnumerable<TargetResponse>> GetByQuestionIdAsync(Guid questionId);
    Task<TargetResponse> CreateAsync(TargetRequest request);
    Task<TargetResponse> UpdateAsync(Guid id, TargetRequest request);
    Task<bool> DeleteAsync(Guid id);
}
