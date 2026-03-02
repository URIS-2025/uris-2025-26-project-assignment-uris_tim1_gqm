using AssessmentService.Application.DTOs;
using AssessmentService.Domain.Entities;

namespace AssessmentService.Application.Mappings;

public static class AssessmentMappings
{
    public static AssessmentResponse ToResponse(this GoalProbabilityAssessment assessment)
        => new(
            assessment.Id,
            assessment.GoalId,
            assessment.Probability,
            assessment.State,
            assessment.Method,
            assessment.Notes
        );

    public static GoalProbabilityAssessment ToEntity(this CreateAssessmentRequest request)
        => new()
        {
            Id = Guid.NewGuid(),
            GoalId = request.GoalId,
            Probability = request.Probability,
            State = request.State,
            Method = request.Method,
            Notes = request.Notes
        };

    public static void UpdateEntity(this UpdateAssessmentRequest request, GoalProbabilityAssessment assessment)
    {
        assessment.Probability = request.Probability;
        assessment.State = request.State;
        assessment.Method = request.Method;
        assessment.Notes = request.Notes;
    }
}