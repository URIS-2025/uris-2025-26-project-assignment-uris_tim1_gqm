namespace AuditService.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; private set; }
    public Guid ActorId { get; private set; }
    public string ActorRole { get; private set; } = default!;
    public string Service { get; private set; } = default!;
    public string Action { get; private set; } = default!;
    public string EntityType { get; private set; } = default!;
    public Guid EntityId { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string? Metadata { get; private set; }

    private AuditLog() { }

    public AuditLog(
        Guid actorId,
        string actorRole,
        string service,
        string action,
        string entityType,
        Guid entityId,
        string? metadata = null)
    {
        Id = Guid.NewGuid();
        ActorId = actorId;
        ActorRole = actorRole;
        Service = service;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        Timestamp = DateTime.UtcNow;
        Metadata = metadata;
    }
}
