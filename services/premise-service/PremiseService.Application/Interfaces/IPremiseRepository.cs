using PremiseService.Domain.Entities;

namespace PremiseService.Application.Interfaces;

public interface IPremiseRepository
{
    Task<IEnumerable<Premise>> GetAllActiveAsync();
    Task<Premise?> GetByIdAsync(Guid id);
    Task<IEnumerable<Premise>> GetByGoalIdAsync(Guid goalId);
    Task<IEnumerable<Premise>> GetByStrategyIdAsync(Guid strategyId);
    Task<IEnumerable<Premise>> GetVersionHistoryAsync(Guid premiseId);
    Task<Premise> CreateAsync(Premise premise);
    Task UpdateAsync(Premise premise);
    Task SaveChangesAsync();
}
