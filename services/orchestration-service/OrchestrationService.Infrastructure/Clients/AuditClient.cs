using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using OrchestrationService.Application.Interfaces.Clients;

namespace OrchestrationService.Infrastructure.Clients;

public class AuditClient : IAuditClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuditClient> _logger;

    public AuditClient(HttpClient httpClient, ILogger<AuditClient> logger)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(2);
        _logger = logger;
    }

    public async Task LogAsync(string action, string entityType, string entityId, string? metadata = null)
    {
        try
        {
            var payload = new
            {
                service = "orchestration-service",
                action,
                entityType,
                entityId,
                timestamp = DateTime.UtcNow,
                metadata
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            await _httpClient.PostAsync("/audit/log", content);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AuditClient.LogAsync failed for action '{Action}'. Swallowing.", action);
        }
    }
}
