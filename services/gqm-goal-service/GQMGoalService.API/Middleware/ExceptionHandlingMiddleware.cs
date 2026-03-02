using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using GQMGoalService.Domain.Exceptions;
using FluentValidationException = FluentValidation.ValidationException;

namespace GQMGoalService.API.Middleware;

/// <summary>
/// Global exception handling middleware that catches unhandled exceptions thrown during
/// request processing and converts them into standardized JSON error responses.
/// </summary>
/// <remarks>
/// Exception → HTTP status code mapping:
/// <list type="bullet">
///   <item><see cref="NotFoundException"/> → 404 Not Found</item>
///   <item><see cref="ConflictException"/> → 409 Conflict</item>
///   <item><see cref="ValidationException"/> / <see cref="FluentValidation.ValidationException"/> → 400 Bad Request</item>
///   <item>All other exceptions → 500 Internal Server Error</item>
/// </list>
/// </remarks>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception has occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            NotFoundException e => new { StatusCode = StatusCodes.Status404NotFound, Message = e.Message, Errors = (object?)null },
            ConflictException e => new { StatusCode = StatusCodes.Status409Conflict, Message = e.Message, Errors = (object?)null },
            ValidationException e => new { StatusCode = StatusCodes.Status400BadRequest, Message = e.Message, Errors = (object?)e.Errors },
            FluentValidationException e => new { 
                StatusCode = StatusCodes.Status400BadRequest, 
                Message = "One or more validation failures have occurred.", 
                Errors = (object?)e.Errors.GroupBy(e => e.PropertyName, e => e.ErrorMessage).ToDictionary(g => g.Key, g => g.ToArray())
            },
            _ => new { StatusCode = StatusCodes.Status500InternalServerError, Message = "An internal server error occurred.", Errors = (object?)null }
        };

        context.Response.StatusCode = response.StatusCode;

        var result = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(result);
    }
}
