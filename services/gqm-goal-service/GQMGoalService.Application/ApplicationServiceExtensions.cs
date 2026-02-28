using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using GQMGoalService.Application.Interfaces;
using GQMGoalService.Application.Mappings;
using GQMGoalService.Application.Services;
using GQMGoalService.Application.Validators;

namespace GQMGoalService.Application;

public static class ApplicationServiceExtensions
{
    /// <summary>
    /// Registers application-layer services: AutoMapper profiles, FluentValidation validators,
    /// and scoped service implementations for dependency injection.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(
            typeof(GqmGoalMappingProfile),
            typeof(QuestionMappingProfile),
            typeof(TargetMappingProfile),
            typeof(MeasurementMappingProfile)
        );

        services.AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters();

        services.AddValidatorsFromAssembly(typeof(GqmGoalRequestValidator).Assembly);

        services.AddScoped<IGqmGoalService, GqmGoalService>();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<ITargetService, TargetService>();
        services.AddScoped<IMeasurementService, MeasurementService>();

        return services;
    }
}
