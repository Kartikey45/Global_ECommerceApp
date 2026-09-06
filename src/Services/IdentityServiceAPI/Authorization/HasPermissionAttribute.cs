using Microsoft.AspNetCore.Authorization;

namespace IdentityServiceAPI.Authorization
{
    /// <summary>
    /// Requires the caller to hold a permission granted to their role.
    ///
    ///     [HasPermission(Permission.Create)]
    ///     [HasPermission(Permission.View)]
    ///
    /// Returns 401 if not authenticated, 403 if authenticated but the
    /// permission is missing.
    ///
    /// Replaces the older DynamicPermissionAttribute: because this
    /// derives from AuthorizeAttribute it participates in the real
    /// authorization pipeline, so it composes with [Authorize],
    /// honours [AllowAnonymous], and is picked up by Swagger.
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
