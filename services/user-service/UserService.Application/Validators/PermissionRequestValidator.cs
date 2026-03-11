using UserService.Application.DTOs;
using FluentValidation;

namespace UserService.Application.Validators;

public class PermissionRequestValidator : AbstractValidator<PermissionRequest>
{
    public PermissionRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Permission name is required.")
            .MaximumLength(100).WithMessage("Permission name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
    }
}
