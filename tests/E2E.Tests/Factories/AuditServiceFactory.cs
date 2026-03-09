using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace E2E.Tests.Factories;

/// <summary>
/// Boots AuditService's full ASP.NET pipeline + MassTransit <c>AuditLogCreatedConsumer</c>
/// against the shared infrastructure containers.
/// AuditService auto-migrates its DB unconditionally, so no manual migration call is needed.
/// </summary>
public sealed class AuditServiceFactory : WebApplicationFactory<AuditService.API.Program>
{
    private readonly string _dbConnectionString;
    private readonly string _rabbitMqUri;
    private readonly string _rabbitMqUsername;
    private readonly string _rabbitMqPassword;

    public AuditServiceFactory(
        string dbConnectionString,
        string rabbitMqUri,
        string rabbitMqUsername,
        string rabbitMqPassword)
    {
        _dbConnectionString = dbConnectionString;
        _rabbitMqUri = rabbitMqUri;
        _rabbitMqUsername = rabbitMqUsername;
        _rabbitMqPassword = rabbitMqPassword;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("DATABASE_URL", _dbConnectionString);
        builder.UseSetting("RabbitMQ:Host", _rabbitMqUri);
        builder.UseSetting("RabbitMQ:Username", _rabbitMqUsername);
        builder.UseSetting("RabbitMQ:Password", _rabbitMqPassword);
        builder.UseSetting("HMAC_SECRET_KEY", "e2e-test-hmac-secret-key-at-least-32-chars!");
    }
}
