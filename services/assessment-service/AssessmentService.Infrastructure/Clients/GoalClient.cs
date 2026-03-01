using System.Text.Json;
using AssessmentService.Application.DTOs.External;
using AssessmentService.Application.Interfaces.Clients;
using Microsoft.Extensions.Logging;

namespace AssessmentService.Infrastructure.Clients;

public class GoalClient : IGoalClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoalClient> _logger;

    public GoalClient(HttpClient httpClient, ILogger<GoalClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<GoalDto?> GetGoalByIdAsync(Guid goalId)
    {
        try
        {
            // TODO: rout should be adjusted
            var response = await _httpClient.GetAsync($"/api/v1/goal/goals/{goalId}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                _logger.LogWarning("Failed to GET goal {GoalId}. Status: {StatusCode}", goalId, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            return JsonSerializer.Deserialize<GoalDto>(content, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching goal {GoalId}", goalId);
            return null;
        }
    }
}