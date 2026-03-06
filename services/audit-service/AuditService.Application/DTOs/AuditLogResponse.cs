namespace AuditService.Application.DTOs;

public record AuditLogResponse(
    Guid Id,
    Guid ActorId,
    string ActorRole,
    string Service,
    string Action,
    string EntityType,
    Guid EntityId,
    DateTime Timestamp,
    string? Metadata
);
