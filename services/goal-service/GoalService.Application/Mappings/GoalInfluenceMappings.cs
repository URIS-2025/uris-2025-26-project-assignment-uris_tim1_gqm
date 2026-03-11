using GoalService.Application.DTOs;
using GoalService.Domain.Entities;
using GoalService.Domain.Enums;

namespace GoalService.Application.Mappings;

public static class GoalInfluenceMappings
{
    public static GoalInfluenceResponse ToResponse(this GoalInfluence influence)
    {
        return new GoalInfluenceResponse
        {
            GoalId = influence.GoalId,
            StrategyId = influence.StrategyId,
            InfluenceType = influence.InfluenceType.ToString(),
            Strength = influence.Strength,
            Confidence = influence.Confidence,
            CreatedAt = influence.CreatedAt,
            Notes = influence.Notes
        };
    }

    public static GoalInfluence ToEntity(this GoalInfluenceRequest request)
    {
        return new GoalInfluence
        {
            GoalId = request.GoalId,
            StrategyId = request.StrategyId,
            InfluenceType = Enum.Parse<InfluenceType>(request.InfluenceType, ignoreCase: true),
            Strength = request.Strength,
            Confidence = request.Confidence,
            CreatedAt = DateTime.UtcNow,
            Notes = request.Notes
        };
    }
}
