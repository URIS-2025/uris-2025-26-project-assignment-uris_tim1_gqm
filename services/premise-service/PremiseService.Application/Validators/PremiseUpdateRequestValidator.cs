using FluentValidation;
using PremiseService.Application.DTOs;

namespace PremiseService.Application.Validators;

public class PremiseUpdateRequestValidator : AbstractValidator<PremiseUpdateRequest>
{
    public PremiseUpdateRequestValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Type must be a valid PremiseType.");

        RuleFor(x => x)
            .Must(x => x.GoalId.HasValue || x.StrategyId.HasValue)
            .WithMessage("At least one of GoalId or StrategyId must be provided.");
    }
}
