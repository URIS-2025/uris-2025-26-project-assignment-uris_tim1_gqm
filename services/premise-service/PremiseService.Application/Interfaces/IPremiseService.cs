using PremiseService.Application.DTOs;

namespace PremiseService.Application.Interfaces;

/// <summary>
/// Service interface defining business operations for the Premise aggregate.
/// Works with DTOs as input/output, hiding domain entities from the API layer.
/// </summary>
public interface IPremiseService
{
    /// <summary>Returns all currently active premises.</summary>
    Task<IEnumerable<PremiseResponse>> GetAllActiveAsync();

    /// <summary>Returns a single premise by its unique identifier.</summary>
    /// <param name="id">The unique identifier of the premise.</param>
    /// <exception cref="Domain.Exceptions.PremiseNotFoundException">Thrown if the premise is not found.</exception>
    Task<PremiseResponse> GetByIdAsync(Guid id);

    /// <summary>Returns all active premises associated with a specific goal.</summary>
    /// <param name="goalId">The goal identifier to filter by.</param>
    Task<IEnumerable<PremiseResponse>> GetByGoalIdAsync(Guid goalId);

    /// <summary>Returns all active premises associated with a specific strategy.</summary>
    /// <param name="strategyId">The strategy identifier to filter by.</param>
    Task<IEnumerable<PremiseResponse>> GetByStrategyIdAsync(Guid strategyId);

    /// <summary>Returns the full version history of a premise, oldest to newest.</summary>
    /// <param name="premiseId">The identifier of the premise.</param>
    /// <exception cref="Domain.Exceptions.PremiseNotFoundException">Thrown if the premise is not found.</exception>
    Task<IEnumerable<PremiseResponse>> GetVersionHistoryAsync(Guid premiseId);

    /// <summary>Creates a new premise.</summary>
    /// <param name="request">The data for the new premise.</param>
    /// <returns>The newly created premise.</returns>
    Task<PremiseResponse> CreateAsync(PremiseRequest request);

    /// <summary>
    /// Creates a new version of an existing premise.
    /// The original premise is deactivated (IsActive = false) and a new version
    /// is created with NewVersionOfId pointing to the original.
    /// </summary>
    /// <param name="id">The identifier of the premise to update.</param>
    /// <param name="request">The updated data (description).</param>
    /// <returns>The newly created version of the premise.</returns>
    /// <exception cref="Domain.Exceptions.PremiseNotFoundException">Thrown if the premise is not found.</exception>
    Task<PremiseResponse> UpdateAsync(Guid id, PremiseUpdateRequest request);

    /// <summary>
    /// Soft-deletes a premise by setting IsActive to false.
    /// The premise remains in the database for historical reference.
    /// </summary>
    /// <param name="id">The identifier of the premise to deactivate.</param>
    /// <exception cref="Domain.Exceptions.PremiseNotFoundException">Thrown if the premise is not found.</exception>
    Task DeactivateAsync(Guid id);
}
