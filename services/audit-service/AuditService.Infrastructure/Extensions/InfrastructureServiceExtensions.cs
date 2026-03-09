using AuditService.Application.Interfaces;
using AuditService.Application.Services;
using AuditService.Infrastructure.Consumers;
using AuditService.Infrastructure.Data;
using MassTransit;
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

        services.AddMassTransit(x =>
        {
            x.AddConsumer<AuditLogCreatedConsumer>();

            x.AddEntityFrameworkOutbox<AuditDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });

            x.UsingRabbitMq((ctx, cfg) =>
            {
                var rabbitMqHost = configuration["RabbitMQ:Host"] ?? "rabbitmq://localhost";
                var rabbitMqUsername = configuration["RabbitMQ:Username"] ?? "guest";
                var rabbitMqPassword = configuration["RabbitMQ:Password"] ?? "guest";

                cfg.Host(rabbitMqHost, h =>
                {
                    h.Username(rabbitMqUsername);
                    h.Password(rabbitMqPassword);
                });

                cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(1)));

                cfg.ConfigureEndpoints(ctx);
            });
        });

        return services;
    }
}
