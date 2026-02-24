using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GQMGoalService.Application.Interfaces;
using GQMGoalService.Infrastructure.Persistence;

namespace GQMGoalService.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        bool inMemory = configuration.GetValue<bool>("UseInMemoryDatabase");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (inMemory)
            {
                options.UseInMemoryDatabase("TestDb");
            }
            else
            {
                var connectionString = configuration["DATABASE_URL"]
                    ?? configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("Database connection string is required.");
                options.UseNpgsql(connectionString);
            }
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        return services;
    }

    public static async Task UseInfrastructureAsync(this Microsoft.AspNetCore.Builder.WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var configuration = app.Services.GetRequiredService<IConfiguration>();

        if (!configuration.GetValue<bool>("UseInMemoryDatabase"))
        {
            var maxRetries = 5;
            var retryDelay = TimeSpan.FromSeconds(5);
            for (var i = 0; i < maxRetries; i++)
            {
                try
                {
                    context.Database.Migrate();
                    break;
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
                    logger.LogWarning(ex, "Failed to connect to database. Retrying in {Delay}s...", retryDelay.TotalSeconds);
                    if (i == maxRetries - 1) throw;
                    await Task.Delay(retryDelay);
                }
            }
        }

        await DataSeeder.SeedAsync(context);
    }
}
