namespace UserService.Application.DTOs;

public record AssignRoleRequest
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public Guid OrganizationId { get; set; }
}
