using System.Text.Json;
using GoalService.Application.DTOs.External;
using GoalService.Application.Interfaces.Clients;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GoalService.Infrastructure.Clients;

public class DepartmentClient : IDepartmentClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<DepartmentClient> _logger;

    public DepartmentClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<DepartmentClient> logger)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<IEnumerable<Guid>> GetMyDepartmentIdsAsync()
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/department?page=1&size=1000");
            
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrWhiteSpace(authHeader))
            {
                request.Headers.Add("Authorization", authHeader);
            }

            // Forward the X-Organization-Id header so OrganizationContextMiddleware
            // on the department service can inject the org claim correctly.
            var orgHeader = _httpContextAccessor.HttpContext?.Request.Headers["X-Organization-Id"].ToString();
            if (!string.IsNullOrWhiteSpace(orgHeader))
            {
                request.Headers.Add("X-Organization-Id", orgHeader);
            }

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch departments from department-service. Status Code: {StatusCode}", response.StatusCode);
                return Enumerable.Empty<Guid>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            
            // Expected response is PaginationResponse<DepartmentResponse>
            // We'll deserialize to a basic structure to extract IDs
            using var document = JsonDocument.Parse(content);
            if (document.RootElement.TryGetProperty("items", out var itemsElement) && itemsElement.ValueKind == JsonValueKind.Array)
            {
                var departmentIds = new List<Guid>();
                foreach (var item in itemsElement.EnumerateArray())
                {
                    if (item.TryGetProperty("id", out var idElement) && Guid.TryParse(idElement.GetString(), out var id))
                    {
                        departmentIds.Add(id);
                    }
                }
                return departmentIds;
            }

            return Enumerable.Empty<Guid>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching departments from department-service.");
            return Enumerable.Empty<Guid>();
        }
    }

    public async Task<DepartmentDto?> GetDepartmentAsync(Guid departmentId)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/department/{departmentId}");
            
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrWhiteSpace(authHeader))
            {
                request.Headers.Add("Authorization", authHeader);
            }

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch department {DepartmentId} from department-service. Status Code: {StatusCode}", 
                    departmentId, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<DepartmentDto>(content, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching department {DepartmentId} from department-service.", departmentId);
            return null;
        }
    }
}
