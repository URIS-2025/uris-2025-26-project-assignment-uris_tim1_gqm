namespace DepartmentService.Application.DTOs;

public class DepartmentRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid OrganizationId { get; set; }
}
