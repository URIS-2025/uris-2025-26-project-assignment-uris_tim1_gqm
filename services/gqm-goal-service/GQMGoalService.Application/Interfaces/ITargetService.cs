using Shared.Contracts;
using GQMGoalService.Application.DTOs;
using GQMGoalService.Application.DTOs.Target;

namespace GQMGoalService.Application.Interfaces;

/// <summary>
/// Defines operations for managing measurement targets within a question.
/// </summary>
public interface ITargetService
{
    Task<PaginationResponse<TargetResponse>> GetAllAsync(PaginationRequest request, CancellationToken cancellationToken = default);
    Task<TargetResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all targets belonging to a specific question.
    /// </summary>
    /// <exception cref="GQMGoalService.Domain.Exceptions.NotFoundException">
    /// Thrown when the parent question does not exist.
    /// </exception>
    Task<IEnumerable<TargetResponse>> GetByQuestionIdAsync(Guid questionId, CancellationToken cancellationToken = default);

    Task<TargetResponse> CreateAsync(TargetRequest request, CancellationToken cancellationToken = default);
    Task<TargetResponse> UpdateAsync(Guid id, TargetRequest request, CancellationToken cancellationToken = default);

    /// <exception cref="GQMGoalService.Domain.Exceptions.ConflictException">
    /// Thrown when the target has associated measurements and cannot be deleted.
    /// </exception>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
