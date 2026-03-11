using DepartmentService.Application.DTOs;
using Shared.Contracts;

namespace DepartmentService.Application.Interfaces;

public interface IDepartmentService
{
    Task<PaginationResponse<DepartmentResponse>> GetAllAsync(int page, int size);
    Task<PaginationResponse<DepartmentResponse>> GetByOrganizationIdAsync(Guid organizationId, int page, int size);
    Task<DepartmentResponse> GetByIdAsync(Guid id);
    Task<DepartmentResponse> CreateAsync(DepartmentRequest request);
    Task<DepartmentResponse> UpdateAsync(Guid id, DepartmentRequest request);
    Task DeleteAsync(Guid id);
}
