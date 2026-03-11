using PremiseService.Application.DTOs;
using Shared.Contracts;

namespace PremiseService.Application.Interfaces;

/// <summary>
/// Service interface defining business operations for the Premise aggregate.
/// </summary>
public interface IPremiseService
{
    /// <summary>Returns a paginated list of all premises.</summary>
    Task<PaginationResponse<PremiseResponse>> GetAllAsync(PaginationRequest request);

    /// <summary>Returns a single premise by its unique identifier.</summary>
    Task<PremiseResponse> GetByIdAsync(Guid id);

    /// <summary>Returns active premises associated with a specific goal.</summary>
    Task<IEnumerable<PremiseActiveResponse>> GetActiveByGoalIdAsync(Guid goalId);

    /// <summary>Returns active premises associated with a specific strategy.</summary>
    Task<IEnumerable<PremiseActiveResponse>> GetActiveByStrategyIdAsync(Guid strategyId);

    /// <summary>Creates a new premise.</summary>
    Task<PremiseResponse> CreateAsync(PremiseRequest request);

    /// <summary>Updates a premise by creating a new version and deactivating the old one.</summary>
    Task<PremiseResponse> UpdateAsync(Guid id, PremiseUpdateRequest request);

    /// <summary>Deletes a premise (soft-delete: sets IsActive to false).</summary>
    Task DeleteAsync(Guid id);
}
