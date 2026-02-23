using GQMGoalService.Application.DTOs;
using GQMGoalService.Application.DTOs.Target;

namespace GQMGoalService.Application.Interfaces;

public interface ITargetService
{
    Task<PagedResult<TargetResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<TargetResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TargetResponse>> GetByQuestionIdAsync(Guid questionId, CancellationToken cancellationToken = default);
    Task<TargetResponse> CreateAsync(TargetRequest request, CancellationToken cancellationToken = default);
    Task<TargetResponse> UpdateAsync(Guid id, TargetRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
