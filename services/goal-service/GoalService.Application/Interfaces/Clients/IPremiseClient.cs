using GoalService.Application.DTOs.External;

namespace GoalService.Application.Interfaces.Clients;

public interface IPremiseClient
{
    Task<IEnumerable<PremiseDto>> GetPremisesForGoalAsync(Guid goalId);
}
