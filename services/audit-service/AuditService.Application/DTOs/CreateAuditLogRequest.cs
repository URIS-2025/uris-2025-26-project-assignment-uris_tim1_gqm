namespace AuditService.Application.DTOs;

public record CreateAuditLogRequest(
    Guid ActorId,
    string ActorRole,
    string Service,
    string Action,
    string EntityType,
    Guid EntityId,
    object? Metadata = null
);
