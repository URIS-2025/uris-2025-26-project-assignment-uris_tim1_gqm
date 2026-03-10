using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace E2E.Tests.Helpers;

/// <summary>
/// Generates valid JWT tokens for E2E test authentication.
/// Uses the same secret/issuer/audience configured in <c>GoalServiceFactory</c>.
/// </summary>
public static class JwtTokenHelper
{
    public const string SecretKey  = "e2e-test-jwt-secret-key-at-least-32-chars!!";
    public const string Issuer     = "e2e-test-issuer";
    public const string Audience   = "e2e-test-audience";

    /// <summary>
    /// Creates a signed JWT bearer token with a standard set of test claims.
    /// </summary>
    public static string GenerateToken(
        Guid?   userId         = null,
        string  role           = "Admin",
        Guid?   organizationId = null,
        string[]? permissions  = null)
    {
        var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,  (userId ?? Guid.NewGuid()).ToString()),
            new(JwtRegisteredClaimNames.Jti,  Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, role),
        };

        if (organizationId.HasValue)
            claims.Add(new Claim("organization_id", organizationId.Value.ToString()));

        foreach (var perm in permissions ?? [
            "manage_goals", "manage_departments", "manage_users",
            "create_goals", "edit_goals", "delete_goals",
            "view_analytics", "manage_organizations"
        ])
            claims.Add(new Claim("permission", perm));

        var token = new JwtSecurityToken(
            issuer:             Issuer,
            audience:           Audience,
            claims:             claims,
            notBefore:          DateTime.UtcNow.AddSeconds(-5),
            expires:            DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
