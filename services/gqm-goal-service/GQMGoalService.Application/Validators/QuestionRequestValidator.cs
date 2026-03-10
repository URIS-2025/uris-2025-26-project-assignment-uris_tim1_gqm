using FluentValidation;
using GQMGoalService.Application.DTOs.Question;

namespace GQMGoalService.Application.Validators;

public class QuestionRequestValidator : AbstractValidator<QuestionRequest>
{
    public QuestionRequestValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Text is required.")
            .MaximumLength(500).WithMessage("Text must not exceed 500 characters.");

        RuleFor(x => x.GqmGoalId)
            .NotEmpty().WithMessage("GqmGoalId is required.");
    }
}
