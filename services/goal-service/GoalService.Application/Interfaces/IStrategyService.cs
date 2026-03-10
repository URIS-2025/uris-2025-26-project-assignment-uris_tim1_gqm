using GoalService.Application.DTOs;

namespace GoalService.Application.Interfaces;

public interface IStrategyService
{
    Task<IEnumerable<StrategyResponse>> GetByGoalIdAsync(Guid goalId);
    Task<IEnumerable<StrategyResponse>> GetByDepartmentIdAsync(Guid departmentId);
    Task<StrategyResponse?> GetByIdAsync(Guid id);
    Task<StrategyResponse> CreateAsync(StrategyRequest request);
    Task<StrategyResponse?> UpdateAsync(Guid id, StrategyRequest request);
    Task<bool> DeleteAsync(Guid id);
}
