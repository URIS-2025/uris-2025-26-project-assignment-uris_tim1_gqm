using GoalService.Application.DTOs;
using GoalService.Domain.Entities;
using GoalService.Domain.Enums;

namespace GoalService.Application.Mappings;

public static class GoalMappings
{
    public static GoalResponse ToResponse(this Goal goal)
    {
        return new GoalResponse
        {
            Id = goal.Id,
            Focus = goal.Focus,
            Object = goal.Object,
            ActiveFrom = goal.ActiveFrom,
            ActiveTo = goal.ActiveTo,
            Magnitude = goal.Magnitude,
            Constraints = goal.Constraints,
            Status = goal.Status.ToString(),
            BaselineProbability = goal.BaselineProbability,
            DepartmentId = goal.DepartmentId,
            Strategies = goal.Strategies?.Select(s => s.ToResponse()).ToList() ?? new(),
            GoalInfluence = goal.GoalInfluence?.ToResponse()
        };
    }

    public static Goal ToEntity(this GoalRequest request)
    {
        return new Goal
        {
            Id = Guid.NewGuid(),
            Focus = request.Focus,
            Object = request.Object,
            ActiveFrom = DateTime.SpecifyKind(request.ActiveFrom, DateTimeKind.Utc),
            ActiveTo = DateTime.SpecifyKind(request.ActiveTo, DateTimeKind.Utc),
            Magnitude = request.Magnitude,
            Constraints = request.Constraints,
            Status = Enum.Parse<GoalStatus>(request.Status, ignoreCase: true),
            BaselineProbability = request.BaselineProbability,
            DepartmentId = request.DepartmentId
        };
    }

    public static void UpdateEntity(this GoalRequest request, Goal goal)
    {
        goal.Focus = request.Focus;
        goal.Object = request.Object;
        goal.ActiveFrom = DateTime.SpecifyKind(request.ActiveFrom, DateTimeKind.Utc);
        goal.ActiveTo = DateTime.SpecifyKind(request.ActiveTo, DateTimeKind.Utc);
        goal.Magnitude = request.Magnitude;
        goal.Constraints = request.Constraints;
        goal.Status = Enum.Parse<GoalStatus>(request.Status, ignoreCase: true);
        goal.BaselineProbability = request.BaselineProbability;
        goal.DepartmentId = request.DepartmentId;
    }
}
