using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Shared.ErrorHandling;

/// <summary>
/// Middleware that ensures every request has an X-Correlation-Id.
/// Reads the correlation ID from the incoming request header (set by nginx or upstream),
/// or generates a new one if missing. Stores it in HttpContext.Items for downstream use
/// and adds it to the response header.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdConstants.HeaderName].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        context.Items[CorrelationIdConstants.ItemKey] = correlationId;

        // Add correlation ID to response headers
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdConstants.HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        // Add correlation ID to logging scope for structured logging
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            [CorrelationIdConstants.LogProperty] = correlationId
        }))
        {
            await _next(context);
        }
    }
}

/// <summary>
/// Constants for the correlation ID header, context item key, and log property.
/// </summary>
public static class CorrelationIdConstants
{
    public const string HeaderName = "X-Correlation-Id";
    public const string ItemKey = "CorrelationId";
    public const string LogProperty = "CorrelationId";
}
