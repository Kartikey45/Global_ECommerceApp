using Microsoft.AspNetCore.Authorization;

namespace ECommerceApp.Shared.Authorization
{
    /// <summary>
    /// Requires the caller to hold a permission granted to their role.
    ///
    ///     [HasPermission(Permission.Create)]
    ///
    /// Returns 401 if not authenticated, 403 if authenticated but the
    /// permission is missing.
    ///
    /// Combine with [Authorize(Roles = ...)] to require both — the two
    /// attributes are ANDed:
    ///
    ///     [Authorize(Roles = Roles.Groups.Admins)]
    ///     [HasPermission(Permission.Create)]
    ///
    /// Replaces DynamicPermissionAttribute: because this derives from
    /// AuthorizeAttribute it runs in the real authorization pipeline,
    /// so it composes with [Authorize], honours [AllowAnonymous], and
    /// is picked up by Swagger.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Method,
        AllowMultiple = true)]
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(string permission)
            : base($"{PermissionPolicyProvider.Prefix}{permission}")
        {
        }
    }
}
