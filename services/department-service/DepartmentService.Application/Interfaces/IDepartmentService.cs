using DepartmentService.Application.DTOs;

namespace DepartmentService.Application.Interfaces;

public interface IDepartmentService
{
    Task<PagedResponse<DepartmentResponse>> GetAllAsync(int page, int size);
    Task<PagedResponse<DepartmentResponse>> GetByOrganizationIdAsync(Guid organizationId, int page, int size);
    Task<DepartmentResponse> GetByIdAsync(Guid id);
    Task<DepartmentResponse> CreateAsync(DepartmentRequest request);
    Task<DepartmentResponse> UpdateAsync(Guid id, DepartmentRequest request);
    Task DeleteAsync(Guid id);
}
