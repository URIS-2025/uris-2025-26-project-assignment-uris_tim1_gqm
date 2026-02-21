using FluentValidation;
using PremiseService.Application.DTOs;

namespace PremiseService.Application.Validators;

public class PremiseRequestValidator : AbstractValidator<PremiseRequest>
{
    public PremiseRequestValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Type must be a valid PremiseType (Assumption or Context).");

        RuleFor(x => x.GoalId)
            .NotEmpty().WithMessage("GoalId is required.");

        RuleFor(x => x.StrategyId)
            .NotEmpty().WithMessage("StrategyId is required.");
    }
}
