using FluentValidation;
using GoalService.Application.DTOs;
using GoalService.Domain.Enums;

namespace GoalService.Application.Validators;

public class GoalRequestValidator : AbstractValidator<GoalRequest>
{
    public GoalRequestValidator()
    {
        RuleFor(x => x.Focus)
            .NotEmpty().WithMessage("Focus is required.")
            .MaximumLength(500).WithMessage("Focus must be at most 500 characters.");

        RuleFor(x => x.Object)
            .NotEmpty().WithMessage("Object is required.")
            .MaximumLength(500).WithMessage("Object must be at most 500 characters.");

        RuleFor(x => x.ActiveFrom)
            .NotEmpty().WithMessage("ActiveFrom date is required.");

        RuleFor(x => x.ActiveTo)
            .NotEmpty().WithMessage("ActiveTo date is required.")
            .GreaterThan(x => x.ActiveFrom).WithMessage("ActiveTo must be after ActiveFrom.");

        RuleFor(x => x.Magnitude)
            .NotEmpty().WithMessage("Magnitude is required.")
            .MaximumLength(500).WithMessage("Magnitude must be at most 500 characters.");

        RuleFor(x => x.Constraints)
            .NotEmpty().WithMessage("Constraints is required.")
            .MaximumLength(1000).WithMessage("Constraints must be at most 1000 characters.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(s => Enum.TryParse<GoalStatus>(s, ignoreCase: true, out _))
            .WithMessage("Status must be one of: Draft, Active, Completed, Cancelled.");

        RuleFor(x => x.BaselineProbability)
            .InclusiveBetween(0m, 1m).WithMessage("BaselineProbability must be between 0.0 and 1.0.");

        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("DepartmentId is required.");
    }
}
