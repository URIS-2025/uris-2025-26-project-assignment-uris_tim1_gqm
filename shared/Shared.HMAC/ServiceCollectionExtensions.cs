using Microsoft.Extensions.DependencyInjection;

namespace Shared.HMAC;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHmacAuthentication(this IServiceCollection services, string secretKey)
    {
        services.AddSingleton(new HmacService(secretKey));
        return services;
    }

    public static IHttpClientBuilder AddHmacHandler(this IHttpClientBuilder builder)
    {
        builder.AddHttpMessageHandler<HmacDelegatingHandler>();
        return builder;
    }
}
