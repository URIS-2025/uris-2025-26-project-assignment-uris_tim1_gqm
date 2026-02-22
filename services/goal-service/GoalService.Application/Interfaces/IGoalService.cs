using GoalService.Application.DTOs;

namespace GoalService.Application.Interfaces;

public interface IGoalService
{
    Task<IEnumerable<GoalResponse>> GetAllAsync();
    Task<GoalResponse?> GetByIdAsync(Guid id);
    Task<GoalResponse> CreateAsync(GoalRequest request);
    Task<GoalResponse?> UpdateAsync(Guid id, GoalRequest request);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<GoalResponse>> GetByDepartmentIdAsync(Guid departmentId);
}
