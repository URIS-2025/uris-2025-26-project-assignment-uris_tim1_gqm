using FluentValidation;
using GoalService.Application.DTOs;
using GoalService.Domain.Enums;

namespace GoalService.Application.Validators;

public class GoalInfluenceRequestValidator : AbstractValidator<GoalInfluenceRequest>
{
    public GoalInfluenceRequestValidator()
    {
        RuleFor(x => x.GoalId)
            .NotEmpty().WithMessage("GoalId is required.");

        RuleFor(x => x.StrategyId)
            .NotEmpty().WithMessage("StrategyId is required.");

        RuleFor(x => x.InfluenceType)
            .NotEmpty().WithMessage("InfluenceType is required.")
            .Must(t => Enum.TryParse<InfluenceType>(t, ignoreCase: true, out _))
            .WithMessage("InfluenceType must be one of: Positive, Negative, Neutral.");

        RuleFor(x => x.Strength)
            .InclusiveBetween(0m, 1m).WithMessage("Strength must be between 0.0 and 1.0.");

        RuleFor(x => x.Confidence)
            .InclusiveBetween(0m, 1m).WithMessage("Confidence must be between 0.0 and 1.0.");
    }
}
