using FluentValidation;
using GQMGoalService.Application.DTOs.Question;
using GQMGoalService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GQMGoalService.Application.Validators;

public class QuestionRequestValidator : AbstractValidator<QuestionRequest>
{
    private readonly IApplicationDbContext _dbContext;

    public QuestionRequestValidator(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;

        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Text is required.")
            .MaximumLength(500).WithMessage("Text must not exceed 500 characters.");

        RuleFor(x => x.GqmGoalId)
            .NotEmpty().WithMessage("GqmGoalId is required.")
            .MustAsync(async (id, ct) => await _dbContext.GqmGoals.AnyAsync(g => g.Id == id, ct))
            .WithMessage("GqmGoalId does not exist.");
    }
}
