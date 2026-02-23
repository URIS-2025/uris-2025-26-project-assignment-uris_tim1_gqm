using GoalService.Application.DTOs.External;

namespace GoalService.Application.DTOs;

public record GoalDetailsResponse : GoalResponse
{
    public IEnumerable<PremiseDto> Premises { get; set; } = new List<PremiseDto>();
    public IEnumerable<AssessmentDto> Assessments { get; set; } = new List<AssessmentDto>();
    public IEnumerable<QgmGoalDto> QgmGoals { get; set; } = new List<QgmGoalDto>();
}
