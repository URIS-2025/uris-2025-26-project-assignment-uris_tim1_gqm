using System.Text;
using System.Text.Json;
using GQMGoalService.Application.Interfaces.Clients;
using Microsoft.Extensions.Logging;

namespace GQMGoalService.Infrastructure.Clients;

public class OrchestrationClient : IOrchestrationClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrchestrationClient> _logger;

    public OrchestrationClient(HttpClient httpClient, ILogger<OrchestrationClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task RecordStepAsync(Guid goalId, string stepName, string compensationEndpoint, string compensationPayload)
    {
        try
        {
            var json = JsonSerializer.Serialize(new { stepName, compensationEndpoint, compensationPayload });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"/workflow/{goalId}/step", content);
            if (!response.IsSuccessStatusCode)
                _logger.LogWarning("Failed to record step '{StepName}' for GoalId {GoalId}. Status: {Status}", stepName, goalId, response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "OrchestrationClient.RecordStepAsync failed for GoalId {GoalId}, step '{StepName}'.", goalId, stepName);
        }
    }
}
