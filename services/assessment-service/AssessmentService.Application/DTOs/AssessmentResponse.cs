using AssessmentService.Domain.Enums;

namespace AssessmentService.Application.DTOs;

public record AssessmentResponse(
    Guid Id,
    Guid GoalId,
    decimal Probability,
    AssessmentState State,
    AssessmentMethod Method,
    string Notes
);
