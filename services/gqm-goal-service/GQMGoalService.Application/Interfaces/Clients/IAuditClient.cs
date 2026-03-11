namespace GQMGoalService.Application.Interfaces.Clients;

public interface IAuditClient
{
    Task LogAsync(Guid actorId, string actorRole, string action, string entityType, Guid entityId, object? metadata = null);
}
