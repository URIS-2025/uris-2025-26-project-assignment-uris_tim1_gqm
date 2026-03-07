using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.ErrorHandling;

/// <summary>
/// Extension methods for registering error handling and correlation ID services.
/// </summary>
public static class ErrorHandlingExtensions
{
    /// <summary>
    /// Registers required services for correlation ID propagation:
    /// IHttpContextAccessor and CorrelationIdDelegatingHandler.
    /// Call this in the service configuration (builder.Services).
    /// </summary>
    public static IServiceCollection AddCorrelationId(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddTransient<CorrelationIdDelegatingHandler>();
        return services;
    }

    /// <summary>
    /// Adds the correlation ID middleware to the request pipeline.
    /// Should be called early in the pipeline, before exception handling.
    /// </summary>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }

    /// <summary>
    /// Adds the standardized global exception handler middleware.
    /// Should be called right after UseCorrelationId.
    /// </summary>
    public static IApplicationBuilder UseStandardizedExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
