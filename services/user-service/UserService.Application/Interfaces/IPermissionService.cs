using UserService.Application.DTOs;

namespace UserService.Application.Interfaces;

public interface IPermissionService
{
    Task<List<PermissionResponse>> GetAllAsync();
    Task<PermissionResponse> GetByIdAsync(Guid id);
    Task<PermissionResponse> CreateAsync(PermissionRequest request);
    Task<PermissionResponse> UpdateAsync(Guid id, PermissionRequest request);
    Task DeleteAsync(Guid id);
}
