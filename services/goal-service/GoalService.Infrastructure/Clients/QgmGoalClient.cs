using System.Text.Json;
using GoalService.Application.DTOs.External;
using GoalService.Application.Interfaces.Clients;
using Microsoft.Extensions.Logging;

namespace GoalService.Infrastructure.Clients;

public class QgmGoalClient : IQgmGoalClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<QgmGoalClient> _logger;

    public QgmGoalClient(HttpClient httpClient, ILogger<QgmGoalClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<QgmGoalDto>> GetQgmGoalsForGoalAsync(Guid goalId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/qgm-goal/goal/{goalId}");
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return Enumerable.Empty<QgmGoalDto>();
                    
                _logger.LogWarning("Failed to GET QGM Goals for goal {GoalId}. Status: {StatusCode}", goalId, response.StatusCode);
                return Enumerable.Empty<QgmGoalDto>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var qgmGoals = JsonSerializer.Deserialize<IEnumerable<QgmGoalDto>>(content, options);
            
            return qgmGoals ?? Enumerable.Empty<QgmGoalDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching QGM Goals for goal {GoalId}", goalId);
            return Enumerable.Empty<QgmGoalDto>();
        }
    }
}
