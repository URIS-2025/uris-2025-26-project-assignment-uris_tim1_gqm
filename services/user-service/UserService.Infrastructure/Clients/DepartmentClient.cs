using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Contracts;
using UserService.Application.DTOs;
using UserService.Application.Interfaces.Clients;

namespace UserService.Infrastructure.Clients;

public class DepartmentClient : IDepartmentClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DepartmentClient> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DepartmentClient(HttpClient httpClient, ILogger<DepartmentClient> logger, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PaginationResponse<OrganizationDto>?> GetOrganizationsAsync(int page = 1, int size = 100)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/organization?page={page}&size={size}");
            
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrWhiteSpace(authHeader))
            {
                request.Headers.Add("Authorization", authHeader);
            }

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PaginationResponse<OrganizationDto>>();
            }
            
            _logger.LogWarning("DepartmentClient.GetOrganizationsAsync returned non-success status code: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DepartmentClient.GetOrganizationsAsync failed");
            return null;
        }
    }
}
