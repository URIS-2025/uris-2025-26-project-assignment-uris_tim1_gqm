using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Shared.Auth;

/// <summary>
/// Authorization filter that verifies the authenticated user holds a specific permission claim.
/// <para>
/// Usage: <c>[RequirePermission("manage_goals")]</c> on a controller action.
/// </para>
/// <para>
/// The filter checks for a <c>"permission"</c> claim whose value matches the required permission
/// (case-insensitive). If the user is not authenticated, it returns <c>401 Unauthorized</c>.
/// If the user lacks the required permission, it returns <c>403 Forbidden</c>.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequirePermissionAttribute : Attribute, IAuthorizationFilter
{
    /// <summary>
    /// The permission name that the user must hold.
    /// </summary>
    public string Permission { get; }

    /// <summary>
    /// Creates a new instance of <see cref="RequirePermissionAttribute"/>.
    /// </summary>
    /// <param name="permission">The required permission name (e.g. <c>"manage_goals"</c>).</param>
    public RequirePermissionAttribute(string permission)
    {
        Permission = permission;
    }

    /// <inheritdoc />
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (user.Identity is null || !user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                error = "Authentication required.",
                statusCode = 401
            });
            return;
        }

        if (!user.HasPermission(Permission))
        {
            context.Result = new ObjectResult(new
            {
                error = $"Missing required permission: '{Permission}'.",
                statusCode = 403
            })
            {
                StatusCode = 403
            };
        }
    }
}
