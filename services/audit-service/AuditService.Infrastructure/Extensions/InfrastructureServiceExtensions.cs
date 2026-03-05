using AuditService.Application.Interfaces;
using AuditService.Application.Services;
using AuditService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuditService.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["DATABASE_URL"]
            ?? throw new InvalidOperationException("DATABASE_URL is not configured.");

        services.AddDbContext<AuditDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<AuditDbContext>());

        services.AddScoped<IAuditLogService, AuditLogService>();

        return services;
    }
}
