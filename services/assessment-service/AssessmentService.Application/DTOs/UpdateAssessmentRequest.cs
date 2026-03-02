using AssessmentService.Domain.Enums;

namespace AssessmentService.Application.DTOs;

public record UpdateAssessmentRequest(
    decimal Probability,
    AssessmentState State,
    AssessmentMethod Method,
    string Notes
);
