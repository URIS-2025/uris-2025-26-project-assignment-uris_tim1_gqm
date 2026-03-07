using FluentValidation;
using GQMGoalService.Application.DTOs.Target;
using GQMGoalService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GQMGoalService.Application.Validators;

public class TargetRequestValidator : AbstractValidator<TargetRequest>
{
    private readonly IApplicationDbContext _dbContext;

    public TargetRequestValidator(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Unit)
            .IsInEnum().WithMessage("Invalid Unit provided.");

        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("QuestionId is required.");
    }
}
