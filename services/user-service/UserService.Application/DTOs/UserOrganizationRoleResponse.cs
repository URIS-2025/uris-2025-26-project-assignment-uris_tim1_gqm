namespace UserService.Application.DTOs;

public record UserOrganizationRoleResponse
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}
