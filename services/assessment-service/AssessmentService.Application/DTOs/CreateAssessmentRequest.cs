using AssessmentService.Domain.Enums;

namespace AssessmentService.Application.DTOs;

public record CreateAssessmentRequest(
    Guid GoalId,
    decimal Probability,
    AssessmentState State,
    AssessmentMethod Method,
    string Notes
);
