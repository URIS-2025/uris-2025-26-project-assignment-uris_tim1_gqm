using Microsoft.Extensions.Logging;
using UserService.Application.Interfaces;

namespace UserService.Application.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        _logger.LogInformation(
            "Password reset email requested for {Email}. Reset link: {ResetLink}",
            toEmail, resetLink);

        return Task.CompletedTask;
    }
}
