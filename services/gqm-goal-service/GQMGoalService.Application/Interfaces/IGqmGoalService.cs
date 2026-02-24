using GQMGoalService.Application.DTOs;
using GQMGoalService.Application.DTOs.GqmGoal;

namespace GQMGoalService.Application.Interfaces;

/// <summary>
/// Defines operations for managing GQM (Goal-Question-Metric) goal entities.
/// </summary>
public interface IGqmGoalService
{
    Task<PagedResult<GqmGoalResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<GqmGoalResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all GQM goals associated with an external goal from the goal-service.
    /// </summary>
    /// <exception cref="GQMGoalService.Domain.Exceptions.NotFoundException">
    /// Thrown when no GQM goals exist for the specified <paramref name="goalId"/>.
    /// </exception>
    Task<IEnumerable<GqmGoalResponse>> GetByGoalIdAsync(Guid goalId, CancellationToken cancellationToken = default);
    Task<GqmGoalResponse> CreateAsync(GqmGoalRequest request, CancellationToken cancellationToken = default);
    Task<GqmGoalResponse> UpdateAsync(Guid id, GqmGoalRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
