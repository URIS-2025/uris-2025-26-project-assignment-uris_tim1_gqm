namespace Shared.Auth;

/// <summary>
/// Strongly-typed configuration for JWT authentication.
/// Bound from the "Jwt" section in appsettings / environment variables.
/// </summary>
public sealed class JwtSettings
{
    /// <summary>
    /// Configuration section name used for binding.
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// The symmetric secret key used to sign and validate JWT tokens.
    /// Must be at least 32 characters for HMAC-SHA256.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// The issuer claim (iss) expected in the token.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// The audience claim (aud) expected in the token.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Access token expiry duration in minutes. Defaults to 15.
    /// </summary>
    public int ExpiryMinutes { get; set; } = 15;
}
