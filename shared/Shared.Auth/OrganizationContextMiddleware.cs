using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Shared.Auth;

/// <summary>
/// Middleware that reads the <c>X-Organization-Id</c> header from the incoming request
/// and appends it as a claim on the authenticated user's identity.
/// <para>
/// This enables downstream services to scope all database queries to the user's organization
/// by calling <see cref="ClaimsPrincipalExtensions.GetOrganizationId"/>.
/// </para>
/// </summary>
public sealed class OrganizationContextMiddleware
{
    /// <summary>
    /// The HTTP header name used to pass the organization context.
    /// </summary>
    public const string HeaderName = "X-Organization-Id";

    private readonly RequestDelegate _next;
    private readonly ILogger<OrganizationContextMiddleware> _logger;

    public OrganizationContextMiddleware(
        RequestDelegate next,
        ILogger<OrganizationContextMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity is { IsAuthenticated: true }
            && context.Request.Headers.TryGetValue(HeaderName, out var headerValue)
            && Guid.TryParse(headerValue, out var organizationId))
        {
            var identity = context.User.Identity as ClaimsIdentity;
            identity?.AddClaim(new Claim(
                ClaimsPrincipalExtensions.OrganizationIdClaimType,
                organizationId.ToString()));

            _logger.LogDebug(
                "Organization context set to {OrganizationId} for user {UserId}",
                organizationId,
                context.User.GetUserIdOrDefault());
        }

        await _next(context);
    }
}
