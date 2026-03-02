using DepartmentService.Application.DTOs;
using Shared.Contracts;

namespace DepartmentService.Application.Interfaces;

public interface IOrganizationService
{
    Task<PaginationResponse<OrganizationResponse>> GetAllAsync(int page, int size);
    Task<OrganizationResponse> GetByIdAsync(Guid id);
    Task<OrganizationResponse> CreateAsync(OrganizationRequest request);
    Task<OrganizationResponse> UpdateAsync(Guid id, OrganizationRequest request);
    Task DeleteAsync(Guid id);
}
