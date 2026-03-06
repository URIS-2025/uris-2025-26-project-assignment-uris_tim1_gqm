namespace OrchestrationService.Application.Interfaces.Clients;

public interface IAuditClient
{
    /// <summary>
    /// Fire-and-forget audit log. Never throws.
    /// </summary>
    Task LogAsync(string action, string entityType, string entityId, string? metadata = null);
}
