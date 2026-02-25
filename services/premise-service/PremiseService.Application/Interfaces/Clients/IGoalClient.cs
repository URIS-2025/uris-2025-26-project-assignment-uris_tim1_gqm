using PremiseService.Application.DTOs.External;

namespace PremiseService.Application.Interfaces.Clients;

public interface IGoalClient
{
    Task<GoalDto?> GetGoalByIdAsync(Guid goalId);
}
