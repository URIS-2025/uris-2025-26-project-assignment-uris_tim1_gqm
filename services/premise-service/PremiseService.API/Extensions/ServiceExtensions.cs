using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PremiseService.Application.Interfaces;
using PremiseService.Application.Mappings;
using PremiseService.Application.Services;
using PremiseService.Infrastructure.Persistence;

namespace PremiseService.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // AutoMapper
        services.AddAutoMapper(typeof(PremiseMappingProfile).Assembly);

        // FluentValidation
        services.AddValidatorsFromAssemblyContaining<PremiseService.Application.Validators.PremiseRequestValidator>();

        // Application services
        services.AddScoped<IPremiseService, PremiseAppService>();

        // Repositories
        services.AddScoped<IPremiseRepository, PremiseRepository>();

        return services;
    }

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
