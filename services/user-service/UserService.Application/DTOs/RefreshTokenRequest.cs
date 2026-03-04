namespace UserService.Application.DTOs;

public record RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
