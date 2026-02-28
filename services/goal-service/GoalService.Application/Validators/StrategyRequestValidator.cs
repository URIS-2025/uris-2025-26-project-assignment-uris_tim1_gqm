using FluentValidation;
using GoalService.Application.DTOs;
using GoalService.Domain.Enums;

namespace GoalService.Application.Validators;

public class StrategyRequestValidator : AbstractValidator<StrategyRequest>
{
    public StrategyRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(300).WithMessage("Name must be at most 300 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(2000).WithMessage("Description must be at most 2000 characters.");

        RuleFor(x => x.Effectiveness)
            .NotEmpty().WithMessage("Effectiveness is required.")
            .Must(e => Enum.TryParse<EffectivenessLevel>(e, ignoreCase: true, out _))
            .WithMessage("Effectiveness must be one of: Low, Medium, High, VeryHigh.");

        RuleFor(x => x.RefinementType)
            .NotEmpty().WithMessage("RefinementType is required.")
            .Must(r => Enum.TryParse<RefinementType>(r, ignoreCase: true, out _))
            .WithMessage("RefinementType must be one of: AND, OR.");

        RuleFor(x => x.GoalId)
            .NotEmpty().WithMessage("GoalId is required.");
    }
}
