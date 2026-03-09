namespace UserService.Application.DTOs;

public record OrganizationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
