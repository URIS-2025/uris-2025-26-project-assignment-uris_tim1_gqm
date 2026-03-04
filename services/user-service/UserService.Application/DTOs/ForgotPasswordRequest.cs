namespace UserService.Application.DTOs;

public record ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}
