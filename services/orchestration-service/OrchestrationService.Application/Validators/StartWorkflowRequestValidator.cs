using FluentValidation;
using OrchestrationService.Application.DTOs;

namespace OrchestrationService.Application.Validators;

public class StartWorkflowRequestValidator : AbstractValidator<StartWorkflowRequest>
{
    public StartWorkflowRequestValidator()
    {
        RuleFor(x => x.GoalId)
            .NotEmpty().WithMessage("GoalId is required.");
    }
}
