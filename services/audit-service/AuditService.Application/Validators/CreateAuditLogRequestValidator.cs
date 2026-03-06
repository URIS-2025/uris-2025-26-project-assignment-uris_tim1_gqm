using AuditService.Application.DTOs;
using FluentValidation;

namespace AuditService.Application.Validators;

public class CreateAuditLogRequestValidator : AbstractValidator<CreateAuditLogRequest>
{
    public CreateAuditLogRequestValidator()
    {
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.ActorRole).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Service).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Action).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EntityId).NotEmpty();
    }
}
