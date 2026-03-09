using Shared.Contracts;
using UserService.Application.DTOs;

namespace UserService.Application.Interfaces.Clients;

public interface IDepartmentClient
{
    Task<PaginationResponse<OrganizationDto>?> GetOrganizationsAsync(int page = 1, int size = 100);
}
