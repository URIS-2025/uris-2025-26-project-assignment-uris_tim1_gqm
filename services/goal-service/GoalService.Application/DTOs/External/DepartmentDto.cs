namespace GoalService.Application.DTOs.External;

public record DepartmentDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid OrganizationId { get; init; }
}
