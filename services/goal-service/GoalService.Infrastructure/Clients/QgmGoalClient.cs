using System.Text.Json;
using GoalService.Application.DTOs.External;
using GoalService.Application.Interfaces.Clients;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GoalService.Infrastructure.Clients;

public class QgmGoalClient : IQgmGoalClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<QgmGoalClient> _logger;

    public QgmGoalClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<QgmGoalClient> logger)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<IEnumerable<QgmGoalDto>> GetQgmGoalsForGoalAsync(Guid goalId)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/gqmgoal/by-goal/{goalId}");
            
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
