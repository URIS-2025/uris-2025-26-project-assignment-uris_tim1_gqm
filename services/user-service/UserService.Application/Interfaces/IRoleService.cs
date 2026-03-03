using UserService.Application.DTOs;

namespace UserService.Application.Interfaces;

public interface IRoleService
{
    Task<List<RoleResponse>> GetAllAsync();
    Task<RoleResponse> GetByIdAsync(Guid id);
    Task<RoleResponse> CreateAsync(RoleRequest request);
    Task<RoleResponse> UpdateAsync(Guid id, RoleRequest request);
    Task DeleteAsync(Guid id);
}
