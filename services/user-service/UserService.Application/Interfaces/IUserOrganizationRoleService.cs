using UserService.Application.DTOs;

namespace UserService.Application.Interfaces;

public interface IUserOrganizationRoleService
{
    Task<UserOrganizationRoleResponse> AssignRoleAsync(AssignRoleRequest request);
    Task RemoveRoleAsync(Guid userId, Guid roleId);
    Task<List<UserOrganizationRoleResponse>> GetByUserIdAsync(Guid userId);
    Task<List<UserOrganizationRoleResponse>> GetByUserAndOrganizationAsync(Guid userId, Guid organizationId);
}
