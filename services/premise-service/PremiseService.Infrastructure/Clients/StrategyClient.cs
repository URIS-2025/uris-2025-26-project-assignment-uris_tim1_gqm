using System.Text.Json;
using Microsoft.Extensions.Logging;
using PremiseService.Application.DTOs.External;
using PremiseService.Application.Interfaces.Clients;

namespace PremiseService.Infrastructure.Clients;

public class StrategyClient : IStrategyClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<StrategyClient> _logger;

    public StrategyClient(HttpClient httpClient, ILogger<StrategyClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<StrategyDto?> GetStrategyByIdAsync(Guid strategyId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/strategies/{strategyId}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                _logger.LogWarning("Failed to GET strategy {StrategyId}. Status: {StatusCode}", strategyId, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<StrategyDto>(content, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching strategy {StrategyId}", strategyId);
            return null;
        }
    }
}
