using GQMGoalService.Application.DTOs;
using GQMGoalService.Application.DTOs.GqmGoal;

namespace GQMGoalService.Application.Interfaces;

public interface IGqmGoalService
{
    Task<PagedResult<GqmGoalResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<GqmGoalResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<GqmGoalResponse>> GetByGoalIdAsync(Guid goalId, CancellationToken cancellationToken = default);
    Task<GqmGoalResponse> CreateAsync(GqmGoalRequest request, CancellationToken cancellationToken = default);
    Task<GqmGoalResponse> UpdateAsync(Guid id, GqmGoalRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
