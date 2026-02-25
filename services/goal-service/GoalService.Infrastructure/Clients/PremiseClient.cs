using System.Text.Json;
using GoalService.Application.DTOs.External;
using GoalService.Application.Interfaces.Clients;
using Microsoft.Extensions.Logging;

namespace GoalService.Infrastructure.Clients;

public class PremiseClient : IPremiseClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PremiseClient> _logger;

    public PremiseClient(HttpClient httpClient, ILogger<PremiseClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<PremiseDto>> GetPremisesForGoalAsync(Guid goalId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/premise/goal/{goalId}");
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return Enumerable.Empty<PremiseDto>();
                    
                _logger.LogWarning("Failed to GET premises for goal {GoalId}. Status: {StatusCode}", goalId, response.StatusCode);
                return Enumerable.Empty<PremiseDto>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var premises = JsonSerializer.Deserialize<IEnumerable<PremiseDto>>(content, options);
            
            return premises ?? Enumerable.Empty<PremiseDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching premises for goal {GoalId}", goalId);
            return Enumerable.Empty<PremiseDto>();
        }
    }
}
