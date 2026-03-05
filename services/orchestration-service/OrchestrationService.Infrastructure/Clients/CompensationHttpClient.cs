using System.Text;
using Microsoft.Extensions.Logging;
using OrchestrationService.Application.Interfaces.Clients;

namespace OrchestrationService.Infrastructure.Clients;

public class CompensationHttpClient : ICompensationHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CompensationHttpClient> _logger;

    public CompensationHttpClient(HttpClient httpClient, ILogger<CompensationHttpClient> logger)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(5);
        _logger = logger;
    }

    public async Task<bool> CallAsync(string endpoint, string payload)
    {
        try
        {
            // Determine HTTP method: "revert" in path → POST, otherwise → DELETE
            HttpResponseMessage response;

            if (endpoint.Contains("revert", StringComparison.OrdinalIgnoreCase))
            {
                var content = new StringContent(payload ?? string.Empty, Encoding.UTF8, "application/json");
                response = await _httpClient.PostAsync(endpoint, content);
            }
            else
            {
                response = await _httpClient.DeleteAsync(endpoint);
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Compensation call to '{Endpoint}' returned {StatusCode}.",
                    endpoint, response.StatusCode);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Compensation call to '{Endpoint}' threw an exception.", endpoint);
            return false;
        }
    }
}
