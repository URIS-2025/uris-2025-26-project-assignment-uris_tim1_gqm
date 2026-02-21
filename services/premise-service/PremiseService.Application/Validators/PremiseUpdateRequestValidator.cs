using FluentValidation;
using PremiseService.Application.DTOs;

namespace PremiseService.Application.Validators;

public class PremiseUpdateRequestValidator : AbstractValidator<PremiseUpdateRequest>
{
    public PremiseUpdateRequestValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");
    }
}
