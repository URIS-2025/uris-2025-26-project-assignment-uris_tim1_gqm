namespace DepartmentService.Application.DTOs;

public record DepartmentResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? ManagerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
