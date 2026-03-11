using FluentValidation;
using OrchestrationService.Application.DTOs;

namespace OrchestrationService.Application.Validators;

public class RecordStepRequestValidator : AbstractValidator<RecordStepRequest>
{
    public RecordStepRequestValidator()
    {
        RuleFor(x => x.StepName)
            .NotEmpty().WithMessage("StepName is required.")
            .MaximumLength(100).WithMessage("StepName must be at most 100 characters.");

        RuleFor(x => x.CompensationEndpoint)
            .NotEmpty().WithMessage("CompensationEndpoint is required.")
            .MaximumLength(500).WithMessage("CompensationEndpoint must be at most 500 characters.");

        RuleFor(x => x.CompensationPayload)
            .MaximumLength(5000).WithMessage("CompensationPayload must be at most 5000 characters.");
    }
}
