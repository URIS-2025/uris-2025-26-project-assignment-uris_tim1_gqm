using Microsoft.AspNetCore.Http;

namespace Shared.ErrorHandling;

/// <summary>
/// DelegatingHandler that propagates the X-Correlation-Id to all outgoing HTTP requests.
/// Uses IHttpContextAccessor to retrieve the correlation ID stored by CorrelationIdMiddleware.
/// </summary>
public class CorrelationIdDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdDelegatingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext?.Items.TryGetValue(CorrelationIdConstants.ItemKey, out var correlationId) == true
            && correlationId is string id
            && !string.IsNullOrWhiteSpace(id))
        {
            request.Headers.Remove(CorrelationIdConstants.HeaderName);
            request.Headers.Add(CorrelationIdConstants.HeaderName, id);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
