using GoalService.Application.DTOs;
using Shared.Contracts;

namespace GoalService.Application.Interfaces;

public interface IGoalService
{
    Task<PaginationResponse<GoalResponse>> GetAllPaginatedAsync(PaginationRequest request);
    Task<GoalResponse?> GetByIdAsync(Guid id);
    Task<GoalResponse> CreateAsync(GoalRequest request);
    Task<GoalResponse?> UpdateAsync(Guid id, GoalRequest request);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<GoalResponse>> GetByDepartmentIdAsync(Guid departmentId);
    Task<GoalDetailsResponse?> GetGoalDetailsAsync(Guid id);
    Task<ActivationReadinessResponse> ReadinessAsync(Guid id);
    Task<GoalResponse?> ActivateAsync(Guid id);
    Task<GoalResponse?> RevertToDraftAsync(Guid id);
    
    // Analytics methods
    Task<IEnumerable<GoalResponse>> GetRootGoalsByDepartmentAsync(Guid departmentId);
    Task<GoalTreeNodeResponse?> GetGoalTreeAsync(Guid rootGoalId);
    Task<GoalAnalyticsResponse> GetAnalyticsAsync(Guid? departmentId, Guid? rootGoalId);
}
