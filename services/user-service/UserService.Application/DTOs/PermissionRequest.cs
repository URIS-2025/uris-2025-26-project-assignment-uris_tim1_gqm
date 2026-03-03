namespace UserService.Application.DTOs;

public record PermissionRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
