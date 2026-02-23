using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using GQMGoalService.Application.Interfaces;
using GQMGoalService.Application.Mappings;
using GQMGoalService.Application.Services;
using GQMGoalService.Application.Validators;
using GQMGoalService.Infrastructure.Persistence;

namespace GQMGoalService.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        services.AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters();
                
        services.AddValidatorsFromAssembly(typeof(GqmGoalRequestValidator).Assembly);

        services.AddScoped<IGqmGoalService, GqmGoalService>();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<ITargetService, TargetService>();
        services.AddScoped<IMeasurementService, MeasurementService>();

        return services;
    }

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

        return services;
    }
}
