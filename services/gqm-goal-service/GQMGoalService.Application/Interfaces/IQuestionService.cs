using Shared.Contracts;
using GQMGoalService.Application.DTOs;
using GQMGoalService.Application.DTOs.Question;

namespace GQMGoalService.Application.Interfaces;

/// <summary>
/// Defines operations for managing questions within a GQM goal.
/// </summary>
public interface IQuestionService
{
    Task<PaginationResponse<QuestionResponse>> GetAllAsync(PaginationRequest request, CancellationToken cancellationToken = default);
    Task<QuestionResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all questions belonging to a specific GQM goal.
    /// </summary>
    /// <exception cref="GQMGoalService.Domain.Exceptions.NotFoundException">
    /// Thrown when the parent GQM goal does not exist.
    /// </exception>
    Task<IEnumerable<QuestionResponse>> GetByGqmGoalIdAsync(Guid gqmGoalId, CancellationToken cancellationToken = default);

    Task<QuestionResponse> CreateAsync(QuestionRequest request, CancellationToken cancellationToken = default);
    Task<QuestionResponse> UpdateAsync(Guid id, QuestionRequest request, CancellationToken cancellationToken = default);

    /// <exception cref="GQMGoalService.Domain.Exceptions.ConflictException">
    /// Thrown when the question has associated targets and cannot be deleted.
    /// </exception>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
