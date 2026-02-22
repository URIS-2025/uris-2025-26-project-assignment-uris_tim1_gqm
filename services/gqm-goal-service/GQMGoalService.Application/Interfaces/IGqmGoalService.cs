using GQMGoalService.Application.DTOs.GqmGoal;

namespace GQMGoalService.Application.Interfaces;

public interface IGqmGoalService
{
    Task<IEnumerable<GqmGoalResponse>> GetAllAsync();
    Task<GqmGoalResponse> GetByIdAsync(Guid id);
    Task<IEnumerable<GqmGoalResponse>> GetByGoalIdAsync(Guid goalId);
    Task<GqmGoalResponse> CreateAsync(GqmGoalRequest request);
    Task<GqmGoalResponse> UpdateAsync(Guid id, GqmGoalRequest request);
    Task<bool> DeleteAsync(Guid id);
}
