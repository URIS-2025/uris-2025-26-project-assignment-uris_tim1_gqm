using UserService.Application.Interfaces.Clients;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace UserService.Infrastructure.Clients;

public class AuditClient : IAuditClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuditClient> _logger;

    public AuditClient(HttpClient httpClient, ILogger<AuditClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task LogAsync(Guid actorId, string actorRole, string action, string entityType, Guid entityId, object? metadata = null)
    {
        try
        {
            var payload = new { actorId, actorRole, service = "user-service", action, entityType, entityId, metadata };
            var json = JsonSerializer.Serialize(payload);
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await _httpClient.PostAsync("/audit/log", content, cts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AuditClient.LogAsync failed for {Action} on {EntityType}/{EntityId}", action, entityType, entityId);
        }
    }
}
