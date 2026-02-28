using Shared.Contracts;
using GQMGoalService.Application.DTOs;
using GQMGoalService.Application.DTOs.Measurement;

namespace GQMGoalService.Application.Interfaces;

/// <summary>
/// Defines operations for managing measurements recorded against a target.
/// </summary>
public interface IMeasurementService
{
    Task<PaginationResponse<MeasurementResponse>> GetAllAsync(PaginationRequest request, CancellationToken cancellationToken = default);
    Task<MeasurementResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all measurements belonging to a specific target.
    /// </summary>
    /// <exception cref="GQMGoalService.Domain.Exceptions.NotFoundException">
    /// Thrown when the parent target does not exist.
    /// </exception>
    Task<IEnumerable<MeasurementResponse>> GetByTargetIdAsync(Guid targetId, CancellationToken cancellationToken = default);

    Task<MeasurementResponse> CreateAsync(MeasurementRequest request, CancellationToken cancellationToken = default);
    Task<MeasurementResponse> UpdateAsync(Guid id, MeasurementRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
