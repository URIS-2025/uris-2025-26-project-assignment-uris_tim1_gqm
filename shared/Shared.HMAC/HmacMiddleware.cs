using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Shared.HMAC;

public class HmacMiddleware
{
    private readonly RequestDelegate _next;
    private readonly HmacService _hmacService;
    private readonly ILogger<HmacMiddleware> _logger;

    public HmacMiddleware(RequestDelegate next, HmacService hmacService, ILogger<HmacMiddleware> logger)
    {
        _next = next;
        _hmacService = hmacService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip HMAC validation for health checks and swagger
        if (context.Request.Path.StartsWithSegments("/health") || 
            context.Request.Path.StartsWithSegments("/swagger") ||
            context.Request.Path.StartsWithSegments("/weatherforecast"))
        {
            await _next(context);
            return;
        }

        // Check if request is from another service (has HMAC headers)
        var signature = context.Request.Headers[HmacService.Headers.Signature].FirstOrDefault();
        var timestamp = context.Request.Headers[HmacService.Headers.Timestamp].FirstOrDefault();

        // If no HMAC headers, allow (could be from gateway/client)
        if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(timestamp))
        {
            await _next(context);
            return;
        }

        // Enable buffering to read body multiple times
        context.Request.EnableBuffering();

        string requestBody;
        using (var reader = new StreamReader(context.Request.Body, leaveOpen: true))
        {
            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0; // Reset for next middleware
        }

        // Validate HMAC
        if (!_hmacService.ValidateSignature(requestBody, signature, timestamp))
        {
            _logger.LogWarning("HMAC validation failed for request from {RemoteIp}", context.Connection.RemoteIpAddress);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid HMAC signature");
            return;
        }

        await _next(context);
    }
}
