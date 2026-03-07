using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Shared.ErrorHandling;

/// <summary>
/// Unified global exception handler middleware for all microservices.
/// Maps exceptions to standardized error responses with consistent format.
/// Uses convention-based exception name matching so each service's domain
/// exceptions work automatically without per-service configuration.
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

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
        var correlationId = context.Items.TryGetValue(CorrelationIdConstants.ItemKey, out var cid)
            ? cid as string ?? "unknown"
            : "unknown";

        var (statusCode, errorResponse) = MapException(exception);

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception,
                "Unhandled exception occurred. CorrelationId: {CorrelationId}", correlationId);
        }
        else
        {
            _logger.LogWarning(exception,
                "Handled exception ({ErrorType}): {Message}. CorrelationId: {CorrelationId}",
                errorResponse.Type, exception.Message, correlationId);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(errorResponse, JsonOptions);
        await context.Response.WriteAsync(json);
    }

    private static (HttpStatusCode statusCode, StandardErrorResponse response) MapException(Exception exception)
    {
        return exception switch
        {
            // FluentValidation errors → 400 with field-level details
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                StandardErrorResponse.Create(
                    ErrorTypes.ValidationError,
                    "One or more validation errors occurred.",
                    validationEx.Errors
                        .GroupBy(e => e.PropertyName ?? "_general")
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        )
                )),

            // ArgumentException → 400
            ArgumentException argEx => (
                HttpStatusCode.BadRequest,
                StandardErrorResponse.Create(
                    ErrorTypes.BadRequest,
                    argEx.Message)),

            // UnauthorizedAccessException → 401
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                StandardErrorResponse.Create(
                    ErrorTypes.Unauthorized,
                    "Authentication is required to access this resource.")),

            // Convention-based mapping using exception type name
            _ => MapByConvention(exception)
        };
    }

    /// <summary>
    /// Maps domain exceptions by their type name to appropriate HTTP responses.
    /// This allows each service's custom exceptions to be handled without explicit registration. 
    /// </summary>
    private static (HttpStatusCode statusCode, StandardErrorResponse response) MapByConvention(Exception exception)
    {
        var typeName = exception.GetType().Name;

        // Not found exceptions (e.g., GoalNotFoundException, PremiseNotFoundException)
        if (typeName.Contains("NotFound", StringComparison.OrdinalIgnoreCase))
        {
            return (HttpStatusCode.NotFound,
                StandardErrorResponse.Create(ErrorTypes.NotFound, exception.Message));
        }

        // Conflict exceptions (e.g., InvalidGoalStateException, GoalHierarchyCycleException, AlreadyExists, AlreadyDeactivated)
        if (typeName.Contains("AlreadyExists", StringComparison.OrdinalIgnoreCase) ||
            typeName.Contains("AlreadyDeactivated", StringComparison.OrdinalIgnoreCase) ||
            typeName.Contains("Conflict", StringComparison.OrdinalIgnoreCase))
        {
            return (HttpStatusCode.Conflict,
                StandardErrorResponse.Create(ErrorTypes.Conflict, exception.Message));
        }

        // Unprocessable entity (e.g., GoalActivationException, InvalidGoalStateException, InvalidProbability)
        if (typeName.Contains("Activation", StringComparison.OrdinalIgnoreCase) ||
            typeName.Contains("InvalidState", StringComparison.OrdinalIgnoreCase) ||
            typeName.Contains("InvalidProbability", StringComparison.OrdinalIgnoreCase) ||
            typeName.Contains("Cycle", StringComparison.OrdinalIgnoreCase))
        {
            var errors = new Dictionary<string, string[]>();

            // Special handling for GoalActivationException which has Blockers
            var blockersProperty = exception.GetType().GetProperty("Blockers");
            if (blockersProperty?.GetValue(exception) is IEnumerable<string> blockers)
            {
                errors["blockers"] = blockers.ToArray();
            }

            return (HttpStatusCode.UnprocessableEntity,
                StandardErrorResponse.Create(ErrorTypes.UnprocessableEntity, exception.Message, errors));
        }

        // Validation exceptions from domain layer
        if (typeName.Contains("Validation", StringComparison.OrdinalIgnoreCase))
        {
            // Check if the domain ValidationException has an Errors property
            var errorsProperty = exception.GetType().GetProperty("Errors");
            var errors = new Dictionary<string, string[]>();

            if (errorsProperty?.GetValue(exception) is IDictionary<string, string[]> domainErrors)
            {
                errors = new Dictionary<string, string[]>(domainErrors);
            }

            return (HttpStatusCode.BadRequest,
                StandardErrorResponse.Create(ErrorTypes.ValidationError, exception.Message, errors));
        }

        // BadRequest exceptions
        if (typeName.Contains("BadRequest", StringComparison.OrdinalIgnoreCase))
        {
            return (HttpStatusCode.BadRequest,
                StandardErrorResponse.Create(ErrorTypes.BadRequest, exception.Message));
        }

        // Forbidden
        if (typeName.Contains("Forbidden", StringComparison.OrdinalIgnoreCase) ||
            typeName.Contains("AccessDenied", StringComparison.OrdinalIgnoreCase))
        {
            return (HttpStatusCode.Forbidden,
                StandardErrorResponse.Create(ErrorTypes.Forbidden, "You do not have permission to access this resource."));
        }

        // Default: Internal Server Error
        return (HttpStatusCode.InternalServerError,
            StandardErrorResponse.Create(
                ErrorTypes.InternalServerError,
                "An unexpected error occurred."));
    }
}
