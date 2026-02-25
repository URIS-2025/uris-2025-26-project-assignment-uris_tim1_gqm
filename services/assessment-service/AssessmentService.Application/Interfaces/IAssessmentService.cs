using AssessmentService.Application.DTOs;

namespace AssessmentService.Application.Interfaces;

public interface IAssessmentService
{
    Task<AssessmentResponse> CreateAsync(CreateAssessmentRequest request);
    Task<AssessmentResponse> GetByIdAsync(Guid id);
    Task<AssessmentResponse?> GetByGoalIdAsync(Guid goalId);
    Task<AssessmentResponse> UpdateAsync(Guid id, UpdateAssessmentRequest request);
    Task DeleteAsync(Guid id);
}
