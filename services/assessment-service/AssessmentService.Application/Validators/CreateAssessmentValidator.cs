using AssessmentService.Application.DTOs;
using FluentValidation;

namespace AssessmentService.Application.Validators;

public class CreateAssessmentValidator : AbstractValidator<CreateAssessmentRequest>
{
    public CreateAssessmentValidator()
    {
        RuleFor(x => x.GoalId)
            .NotEmpty()
            .WithMessage("GoalId is required.");

        RuleFor(x => x.Probability)
            .InclusiveBetween(0.0m, 1.0m)
            .WithMessage("Probability must be between 0 and 1.");

        RuleFor(x => x.State)
            .IsInEnum()
            .WithMessage("Invalid assessment state.");

        RuleFor(x => x.Method)
            .IsInEnum()
            .WithMessage("Invalid assessment method.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .WithMessage("Notes cannot exceed 2000 characters.");
    }
}
