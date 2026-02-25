using System.Net;
using System.Text.Json;
using AssessmentService.Domain.Exceptions;

namespace AssessmentService.API.Middleware;

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
        catch (AssessmentNotFoundException ex)
        {
            _logger.LogWarning(ex, "Assessment not found.");
            await WriteErrorResponse(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (AssessmentByGoalNotFoundException ex)
        {
            _logger.LogWarning(ex, "Assessment not found for goal.");
            await WriteErrorResponse(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (AssessmentAlreadyExistsException ex)
        {
            _logger.LogWarning(ex, "Assessment already exists for goal.");
            await WriteErrorResponse(context, HttpStatusCode.Conflict, ex.Message);
        }
        catch (InvalidProbabilityException ex)
        {
            _logger.LogWarning(ex, "Invalid probability value.");
            await WriteErrorResponse(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred.");
            await WriteErrorResponse(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new { StatusCode = (int)statusCode, Message = message };
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        await context.Response.WriteAsync(json);
    }
}
