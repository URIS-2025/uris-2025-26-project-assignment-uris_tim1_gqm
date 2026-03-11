using DepartmentService.Application.DTOs;
using FluentValidation;

namespace DepartmentService.Application.Validators;

public class OrganizationRequestValidator : AbstractValidator<OrganizationRequest>
{
    public OrganizationRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Organization name is required.")
            .MaximumLength(200).WithMessage("Organization name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");
    }
}
