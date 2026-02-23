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
            .MustAsync(GqmGoalExists).WithMessage("The specified GqmGoalId does not exist.");
    }

    private async Task<bool> GqmGoalExists(Guid gqmGoalId, CancellationToken cancellationToken)
    {
        return await _dbContext.GqmGoals.AnyAsync(g => g.Id == gqmGoalId, cancellationToken);
    }
}
