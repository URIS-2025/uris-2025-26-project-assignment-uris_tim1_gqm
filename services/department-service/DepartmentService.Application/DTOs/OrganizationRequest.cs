namespace DepartmentService.Application.DTOs;

public record OrganizationRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
