namespace AuditService.Application.DTOs;

public record AuditLogFilter(
    string? Service = null,
    string? Action = null,
    string? EntityType = null,
    Guid? ActorId = null,
    Guid? EntityId = null,
    DateTime? From = null,
    DateTime? To = null
);
