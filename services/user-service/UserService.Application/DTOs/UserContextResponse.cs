namespace UserService.Application.DTOs;

public record UserContextResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Guid? OrganizationId { get; set; }
    public List<string> Permissions { get; set; } = new();
    public List<Guid> ManagedDepartmentIds { get; set; } = new();
}
