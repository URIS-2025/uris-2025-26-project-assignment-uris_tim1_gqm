namespace Shared.Contracts.Messages;

/// <summary>
/// Published by any service that needs to record an audit log entry.
/// Consumed exclusively by AuditService.
/// </summary>
public interface IAuditLogCreated
{
    Guid CorrelationId { get; }
    Guid ActorId { get; }
    string ActorRole { get; }
    string Service { get; }
    string Action { get; }
    string EntityType { get; }
    Guid EntityId { get; }
    string? Metadata { get; }
    DateTime OccurredAt { get; }
}
