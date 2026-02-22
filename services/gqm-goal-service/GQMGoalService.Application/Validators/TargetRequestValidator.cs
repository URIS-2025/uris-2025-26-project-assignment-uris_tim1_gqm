using FluentValidation;
using GQMGoalService.Application.DTOs.Target;
using GQMGoalService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GQMGoalService.Application.Validators;

public class TargetRequestValidator : AbstractValidator<TargetRequest>
{
    private readonly ApplicationDbContext _dbContext;

    public TargetRequestValidator(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Unit)
            .IsInEnum().WithMessage("Invalid Unit provided.");

        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("QuestionId is required.")
            .MustAsync(QuestionExists).WithMessage("The specified QuestionId does not exist.");
    }

    private async Task<bool> QuestionExists(Guid questionId, CancellationToken cancellationToken)
    {
        return await _dbContext.Questions.AnyAsync(q => q.Id == questionId, cancellationToken);
    }
}
