using GoalService.Application.DTOs.External;

namespace GoalService.Application.DTOs;

public record GoalDetailsResponse : GoalResponse
{
    public IEnumerable<PremiseDto> Premises { get; init; } = new List<PremiseDto>();
    public IEnumerable<AssessmentDto> Assessments { get; init; } = new List<AssessmentDto>();
    public IEnumerable<QgmGoalDto> QgmGoals { get; init; } = new List<QgmGoalDto>();
}
