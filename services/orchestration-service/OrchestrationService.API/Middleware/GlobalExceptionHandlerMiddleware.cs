using System.Net;
using System.Text.Json;
using FluentValidation;
using OrchestrationService.Domain.Exceptions;

namespace OrchestrationService.API.Middleware;

/// <summary>
/// Global error handling middleware. Catches domain exceptions and unhandled errors,
/// returning consistent JSON error responses.
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        object? errorDetail = null;

        var (statusCode, message) = exception switch
        {
            SagaNotFoundException ex => (HttpStatusCode.NotFound, ex.Message),
            SagaAlreadyExistsException ex => (HttpStatusCode.Conflict, ex.Message),
            SagaAlreadyCompensatedException ex => (HttpStatusCode.Conflict, ex.Message),
            ValidationException ex => HandleValidation(ex, out errorDetail),
            ArgumentException ex => (HttpStatusCode.BadRequest, ex.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception occurred.");
        else
            _logger.LogWarning(exception, "Domain exception: {Message}", exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = errorDetail is not null
            ? new { status = (int)statusCode, error = statusCode.ToString(), message, errors = errorDetail }
            : (object)new { status = (int)statusCode, error = statusCode.ToString(), message };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }

    private static (HttpStatusCode, string) HandleValidation(ValidationException ex, out object? detail)
    {
        detail = ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        return (HttpStatusCode.UnprocessableEntity, "Validation failed.");
    }
}
