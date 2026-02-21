using PremiseService.Domain.Entities;

namespace PremiseService.Application.Interfaces;

/// <summary>
/// Repository interface for premise data access operations.
/// Implemented in the Infrastructure layer.
/// </summary>
public interface IPremiseRepository
{
    /// <summary>Returns all premises that are currently active.</summary>
    Task<IEnumerable<Premise>> GetAllActiveAsync();

    /// <summary>Returns a premise by its unique identifier, or null if not found.</summary>
    /// <param name="id">The unique identifier of the premise.</param>
    Task<Premise?> GetByIdAsync(Guid id);

    /// <summary>Returns all active premises associated with a specific goal.</summary>
    /// <param name="goalId">The goal identifier to filter by.</param>
    Task<IEnumerable<Premise>> GetByGoalIdAsync(Guid goalId);

    /// <summary>Returns all active premises associated with a specific strategy.</summary>
    /// <param name="strategyId">The strategy identifier to filter by.</param>
    Task<IEnumerable<Premise>> GetByStrategyIdAsync(Guid strategyId);

    /// <summary>
    /// Returns the full version history chain for a given premise,
    /// ordered from oldest to newest.
    /// </summary>
    /// <param name="premiseId">The identifier of any premise in the version chain.</param>
    Task<IEnumerable<Premise>> GetVersionHistoryAsync(Guid premiseId);

    /// <summary>Adds a new premise to the database context.</summary>
    /// <param name="premise">The premise entity to create.</param>
    Task<Premise> CreateAsync(Premise premise);

    /// <summary>Marks an existing premise entity as modified in the database context.</summary>
    /// <param name="premise">The premise entity to update.</param>
    Task UpdateAsync(Premise premise);

    /// <summary>Persists all pending changes to the database.</summary>
    Task SaveChangesAsync();
}
