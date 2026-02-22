using FluentValidation;
using GQMGoalService.Application.DTOs.Measurement;
using GQMGoalService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GQMGoalService.Application.Validators;

public class MeasurementRequestValidator : AbstractValidator<MeasurementRequest>
{
    private readonly ApplicationDbContext _dbContext;

    public MeasurementRequestValidator(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;

        RuleFor(x => x.Value)
            .GreaterThanOrEqualTo(0).WithMessage("Value must be greater than or equal to 0."); // Based on typical measurements, adjust if negatives allowed

        RuleFor(x => x.TargetId)
            .NotEmpty().WithMessage("TargetId is required.")
            .MustAsync(TargetExists).WithMessage("The specified TargetId does not exist.");
    }

    private async Task<bool> TargetExists(Guid targetId, CancellationToken cancellationToken)
    {
        return await _dbContext.Targets.AnyAsync(t => t.Id == targetId, cancellationToken);
    }
}
