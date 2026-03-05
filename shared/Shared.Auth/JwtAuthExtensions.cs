using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Shared.Auth;

/// <summary>
/// Extension methods for registering JWT authentication and authorization
/// in the ASP.NET Core dependency injection container.
/// </summary>
public static class JwtAuthExtensions
{
    /// <summary>
    /// Registers JWT Bearer authentication and authorization using settings
    /// from the <c>Jwt</c> configuration section.
    /// <para>
    /// Expected configuration keys:
    /// <list type="bullet">
    ///   <item><c>Jwt:SecretKey</c> — symmetric signing key (min 32 chars)</item>
    ///   <item><c>Jwt:Issuer</c> — token issuer</item>
    ///   <item><c>Jwt:Audience</c> — token audience</item>
    ///   <item><c>Jwt:ExpiryMinutes</c> — (optional) token lifetime, defaults to 15</item>
    /// </list>
    /// </para>
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <c>Jwt:SecretKey</c> is not configured.
    /// </exception>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = new JwtSettings();
        configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);

        if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
            throw new InvalidOperationException(
                "Jwt:SecretKey is not configured. Provide it via appsettings or environment variables.");

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        return services;
    }
}
