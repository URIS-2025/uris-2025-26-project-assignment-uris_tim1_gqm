using PremiseService.Application.DTOs;

namespace PremiseService.Application.Interfaces;

public interface IPremiseService
{
    Task<IEnumerable<PremiseResponse>> GetAllActiveAsync();
    Task<PremiseResponse> GetByIdAsync(Guid id);
    Task<IEnumerable<PremiseResponse>> GetByGoalIdAsync(Guid goalId);
    Task<IEnumerable<PremiseResponse>> GetByStrategyIdAsync(Guid strategyId);
    Task<IEnumerable<PremiseResponse>> GetVersionHistoryAsync(Guid premiseId);
    Task<PremiseResponse> CreateAsync(PremiseRequest request);
    Task<PremiseResponse> UpdateAsync(Guid id, PremiseUpdateRequest request);
    Task DeactivateAsync(Guid id);
}
