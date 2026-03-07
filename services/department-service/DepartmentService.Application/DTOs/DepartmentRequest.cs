namespace DepartmentService.Application.DTOs;

public record DepartmentRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? ManagerId { get; set; }
}
