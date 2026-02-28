using GoalService.Application.DTOs;
using GoalService.Domain.Entities;
using GoalService.Domain.Enums;

namespace GoalService.Application.Mappings;

public static class StrategyMappings
{
    public static StrategyResponse ToResponse(this Strategy strategy)
    {
        return new StrategyResponse
        {
            Id = strategy.Id,
            Name = strategy.Name,
            Description = strategy.Description,
            Effectiveness = strategy.Effectiveness.ToString(),
            RefinementType = strategy.RefinementType.ToString(),
            GoalId = strategy.GoalId,
            GoalInfluences = strategy.GoalInfluences?.Select(gi => gi.ToResponse()).ToList() ?? new()
        };
    }

    public static Strategy ToEntity(this StrategyRequest request)
    {
        return new Strategy
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Effectiveness = Enum.Parse<EffectivenessLevel>(request.Effectiveness, ignoreCase: true),
            RefinementType = Enum.Parse<RefinementType>(request.RefinementType, ignoreCase: true),
            GoalId = request.GoalId
        };
    }

    public static void UpdateEntity(this StrategyRequest request, Strategy strategy)
    {
        strategy.Name = request.Name;
        strategy.Description = request.Description;
        strategy.Effectiveness = Enum.Parse<EffectivenessLevel>(request.Effectiveness, ignoreCase: true);
        strategy.RefinementType = Enum.Parse<RefinementType>(request.RefinementType, ignoreCase: true);
        strategy.GoalId = request.GoalId;
    }
}
