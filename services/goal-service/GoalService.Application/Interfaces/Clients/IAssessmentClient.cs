using GoalService.Application.DTOs.External;

namespace GoalService.Application.Interfaces.Clients;

public interface IAssessmentClient
{
    Task<IEnumerable<AssessmentDto>> GetAssessmentsForGoalAsync(Guid goalId);
}
