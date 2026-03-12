using System.Text.Json;
using GoalService.Application.DTOs.External;
using GoalService.Application.Interfaces.Clients;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GoalService.Infrastructure.Clients;

public class PremiseClient : IPremiseClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<PremiseClient> _logger;

    public PremiseClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<PremiseClient> logger)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<IEnumerable<PremiseDto>> GetPremisesForGoalAsync(Guid goalId)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/premise/active/goal/{goalId}");
            
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrWhiteSpace(authHeader))
            {
                request.Headers.Add("Authorization", authHeader);
            }

            var orgHeader = _httpContextAccessor.HttpContext?.Request.Headers["X-Organization-Id"].ToString();
            if (!string.IsNullOrWhiteSpace(orgHeader))
            {
                request.Headers.Add("X-Organization-Id", orgHeader);
            }

            var response = await _httpClient.SendAsync(request);
            
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
