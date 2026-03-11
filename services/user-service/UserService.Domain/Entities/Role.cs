namespace UserService.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<UserOrganizationRole> UserOrganizationRoles { get; set; } = new List<UserOrganizationRole>();
}
