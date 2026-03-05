using System.Security.Claims;

namespace Shared.Auth;

/// <summary>
/// Extension methods for reading well-known claims from a <see cref="ClaimsPrincipal"/>.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Custom claim type used for permissions embedded in the JWT token.
    /// </summary>
    public const string PermissionClaimType = "permission";

    /// <summary>
    /// Custom claim type used for the organization context.
    /// </summary>
    public const string OrganizationIdClaimType = "organization_id";

    /// <summary>
    /// Returns the authenticated user's ID from the <see cref="ClaimTypes.NameIdentifier"/> claim.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">Thrown when the claim is missing or not a valid GUID.</exception>
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(value) || !Guid.TryParse(value, out var userId))
            throw new UnauthorizedAccessException("User ID claim is missing or invalid.");

        return userId;
    }

    /// <summary>
    /// Returns the authenticated user's ID, or <c>null</c> if the claim is absent.
    /// </summary>
    public static Guid? GetUserIdOrDefault(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(value, out var userId) ? userId : null;
    }

    /// <summary>
    /// Returns the authenticated user's email from the <see cref="ClaimTypes.Email"/> claim.
    /// </summary>
    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Returns all role names assigned to the authenticated user.
    /// </summary>
    public static IReadOnlyList<string> GetRoles(this ClaimsPrincipal principal)
    {
        return principal.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Returns all permission names assigned to the authenticated user.
    /// </summary>
    public static IReadOnlyList<string> GetPermissions(this ClaimsPrincipal principal)
    {
        return principal.FindAll(PermissionClaimType)
            .Select(c => c.Value)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Checks whether the authenticated user holds a specific permission.
    /// </summary>
    public static bool HasPermission(this ClaimsPrincipal principal, string permission)
    {
        return principal.FindAll(PermissionClaimType)
            .Any(c => c.Value.Equals(permission, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Returns the organization ID from the <see cref="OrganizationIdClaimType"/> claim,
    /// which is populated by the <see cref="OrganizationContextMiddleware"/>.
    /// </summary>
    public static Guid? GetOrganizationId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirst(OrganizationIdClaimType)?.Value;
        return Guid.TryParse(value, out var orgId) ? orgId : null;
    }
}
