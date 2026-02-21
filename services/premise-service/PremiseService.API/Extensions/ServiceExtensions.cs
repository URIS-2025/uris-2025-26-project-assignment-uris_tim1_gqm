using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PremiseService.Application.Interfaces;
using PremiseService.Application.Mappings;
using PremiseService.Application.Services;
using PremiseService.Infrastructure.Persistence;

namespace PremiseService.API.Extensions;

/// <summary>
/// Extension methods for configuring dependency injection in the Premise Service.
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Registers application-level services: AutoMapper, FluentValidation,
    /// business services, and repositories.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // AutoMapper profiles
        services.AddAutoMapper(typeof(PremiseMappingProfile).Assembly);

        // FluentValidation validators
        services.AddValidatorsFromAssemblyContaining<PremiseService.Application.Validators.PremiseRequestValidator>();

        // Application services
        services.AddScoped<IPremiseService, PremiseAppService>();

        // Repositories
        services.AddScoped<IPremiseRepository, PremiseRepository>();

        return services;
    }

    /// <summary>
    /// Configures the PostgreSQL database connection using Entity Framework Core.
    /// Reads the connection string from DATABASE_URL (Docker) or ConnectionStrings:DefaultConnection (local).
    /// </summary>
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["DATABASE_URL"]
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Database connection string not configured");

        services.AddDbContext<PremiseDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }
}
