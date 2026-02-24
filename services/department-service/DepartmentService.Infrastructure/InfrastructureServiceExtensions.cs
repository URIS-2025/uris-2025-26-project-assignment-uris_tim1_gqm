using DepartmentService.Application.Interfaces;
using DepartmentService.Application.Services;
using DepartmentService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DepartmentService.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["DATABASE_URL"]
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Database connection string not configured.");

        services.AddDbContext<DepartmentServiceDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<DbContext>(provider =>
            provider.GetRequiredService<DepartmentServiceDbContext>());

        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IDepartmentService, DepartmentAppService>();

        return services;
    }
}
