using AssessmentService.Application.DTOs.External;

namespace AssessmentService.Application.Interfaces.Clients;

public interface IGoalClient
{
    Task<GoalDto?> GetGoalByIdAsync(Guid goalId);
}