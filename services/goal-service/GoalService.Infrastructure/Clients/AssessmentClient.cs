using System.Text.Json;
using GoalService.Application.DTOs.External;
using GoalService.Application.Interfaces.Clients;
using Microsoft.Extensions.Logging;

namespace GoalService.Infrastructure.Clients;

public class AssessmentClient : IAssessmentClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AssessmentClient> _logger;

    public AssessmentClient(HttpClient httpClient, ILogger<AssessmentClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<AssessmentDto>> GetAssessmentsForGoalAsync(Guid goalId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/assessment/goal/{goalId}");
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return Enumerable.Empty<AssessmentDto>();
                    
                _logger.LogWarning("Failed to GET assessments for goal {GoalId}. Status: {StatusCode}", goalId, response.StatusCode);
                return Enumerable.Empty<AssessmentDto>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var assessments = JsonSerializer.Deserialize<IEnumerable<AssessmentDto>>(content, options);
            
            return assessments ?? Enumerable.Empty<AssessmentDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching assessments for goal {GoalId}", goalId);
            return Enumerable.Empty<AssessmentDto>();
        }
    }
}
