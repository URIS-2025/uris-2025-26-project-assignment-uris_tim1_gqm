using FluentValidation;
using GQMGoalService.Application.DTOs.GqmGoal;

namespace GQMGoalService.Application.Validators;

public class GqmGoalRequestValidator : AbstractValidator<GqmGoalRequest>
{
    public GqmGoalRequestValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(x => x.GoalId)
            .NotEmpty().WithMessage("GoalId is required.");
    }
}
