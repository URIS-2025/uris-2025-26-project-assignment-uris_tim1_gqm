using AssessmentService.Application.Interfaces;
using AssessmentService.Application.Services;
using AssessmentService.Application.Validators;
using AssessmentService.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AssessmentService.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAssessmentService, AssessmentServiceImpl>();
        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["DATABASE_URL"]
            ?? throw new InvalidOperationException("DATABASE_URL is not configured.");

        services.AddDbContext<AssessmentDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IAssessmentDbContext>(provider =>
            provider.GetRequiredService<AssessmentDbContext>());

        return services;
    }

    public static IServiceCollection AddValidationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateAssessmentValidator>();
        return services;
    }
}
