using GoalService.Application.DTOs;

namespace GoalService.Application.Interfaces;

public interface IGoalInfluenceService
{
    Task<IEnumerable<GoalInfluenceResponse>> GetByStrategyIdAsync(Guid strategyId);
    Task<GoalInfluenceResponse?> GetByGoalIdAsync(Guid goalId);
    Task<GoalInfluenceResponse> CreateAsync(GoalInfluenceRequest request);
    Task<bool> DeleteAsync(Guid goalId);
}
