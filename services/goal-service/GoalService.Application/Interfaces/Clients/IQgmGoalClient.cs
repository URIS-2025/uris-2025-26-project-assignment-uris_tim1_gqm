using GoalService.Application.DTOs.External;

namespace GoalService.Application.Interfaces.Clients;

public interface IQgmGoalClient
{
    Task<IEnumerable<QgmGoalDto>> GetQgmGoalsForGoalAsync(Guid goalId);
}
