using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using OrchestrationService.Infrastructure.Persistence;

namespace E2E.Tests.Factories;

/// <summary>
/// Boots OrchestrationService's full ASP.NET pipeline + MassTransit
/// <c>WorkflowTransitionRequestedConsumer</c> against the shared infrastructure containers.
/// OrchestrationService only auto-migrates in Development, so we call
/// <see cref="InitializeDatabaseAsync"/> manually.
/// </summary>
public sealed class OrchestrationServiceFactory
    : WebApplicationFactory<OrchestrationService.API.Program>
{
    private readonly string _dbConnectionString;
    private readonly string _rabbitMqUri;
    private readonly string _rabbitMqUsername;
    private readonly string _rabbitMqPassword;

    public OrchestrationServiceFactory(
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
        builder.UseSetting("Services:AuditService", "http://localhost");
    }

    /// <summary>
    /// Runs EF Core migrations against the test database.
    /// Must be called once before tests run.
    /// </summary>
    public async Task InitializeDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrchestrationDbContext>();
        await db.Database.MigrateAsync();
    }
}
