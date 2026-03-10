using FluentValidation;
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

        // Note: validators are invoked manually via ValidateAndThrowAsync in each service.
        // Do NOT use AddFluentValidationAutoValidation() — it runs validators synchronously
        // before actions, which throws AsyncValidatorInvokedSynchronouslyException because
        // AbstractValidator<T> implements IAsyncValidator<T>.
        services.AddValidatorsFromAssembly(typeof(GqmGoalRequestValidator).Assembly);

        services.AddScoped<IGqmGoalService, GqmGoalService>();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<ITargetService, TargetService>();
        services.AddScoped<IMeasurementService, MeasurementService>();

        return services;
    }
}
