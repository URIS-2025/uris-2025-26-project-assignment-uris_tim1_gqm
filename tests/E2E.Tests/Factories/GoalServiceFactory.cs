using GoalService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace E2E.Tests.Factories;

/// <summary>
/// Boots GoalService's full ASP.NET pipeline (controllers, MassTransit publisher, EF Core)
/// against the shared Testcontainers PostgreSQL and RabbitMQ.
/// </summary>
public sealed class GoalServiceFactory : WebApplicationFactory<GoalService.API.Program>
{
    private readonly string _dbConnectionString;
    private readonly string _rabbitMqUri;
    private readonly string _rabbitMqUsername;
    private readonly string _rabbitMqPassword;

    public GoalServiceFactory(
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
        // Set environment to Testing to avoid running GoalDbSeeder
        builder.UseEnvironment("Testing");

        builder.UseSetting("DATABASE_URL", _dbConnectionString);
        builder.UseSetting("RabbitMQ:Host", _rabbitMqUri);
        builder.UseSetting("RabbitMQ:Username", _rabbitMqUsername);
        builder.UseSetting("RabbitMQ:Password", _rabbitMqPassword);
        builder.UseSetting("HMAC_SECRET_KEY", "e2e-test-hmac-secret-key-at-least-32-chars!");
        builder.UseSetting("Jwt:SecretKey", "e2e-test-jwt-secret-key-at-least-32-chars!!");
        builder.UseSetting("Jwt:Issuer", "e2e-test-issuer");
        builder.UseSetting("Jwt:Audience", "e2e-test-audience");
        builder.UseSetting("Services:PremiseService", "http://localhost");
        builder.UseSetting("Services:AssessmentService", "http://localhost");
        builder.UseSetting("Services:QgmGoalService", "http://localhost");
        builder.UseSetting("Services:OrchestrationService", "http://localhost");
        builder.UseSetting("Services:AuditService", "http://localhost");

        builder.ConfigureServices(services =>
        {
            services.PostConfigure<OutboxDeliveryServiceOptions>(options =>
            {
                options.QueryDelay = TimeSpan.FromMilliseconds(200);
            });
        });
    }

    /// <summary>
    /// Creates and migrates the database. Must be called once before tests run.
    /// </summary>
    public async Task InitializeDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GoalDbContext>();
        await db.Database.MigrateAsync();
    }
}
