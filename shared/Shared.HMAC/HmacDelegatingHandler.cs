using System.Net.Http;
using System.Text;

namespace Shared.HMAC;

public class HmacDelegatingHandler : DelegatingHandler
{
    private readonly HmacService _hmacService;

    public HmacDelegatingHandler(HmacService hmacService)
    {
        _hmacService = hmacService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var timestamp = _hmacService.GetTimestamp();
        var requestBody = string.Empty;

        if (request.Content != null)
        {
            requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
        }

        var signature = _hmacService.ComputeSignature(requestBody, timestamp);

        request.Headers.Add(HmacService.Headers.Signature, signature);
        request.Headers.Add(HmacService.Headers.Timestamp, timestamp);

        return await base.SendAsync(request, cancellationToken);
    }
}
