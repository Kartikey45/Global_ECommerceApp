using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerceApp.Shared.Attributes
{
    /// <summary>
    /// Matches IdentityServiceAPI's DynamicPermissionAttribute exactly.
    /// Reads "Permission" claims from the JWT token.
    /// Usage: [DynamicPermission("View")] / [DynamicPermission("Create")]
    ///        [DynamicPermission("Edit")] / [DynamicPermission("Delete")]
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Method | AttributeTargets.Class,
        AllowMultiple = true)]
    public class DynamicPermissionAttribute
        : Attribute, IAuthorizationFilter
    {
        private readonly string _requiredPermission;

        public DynamicPermissionAttribute(
            string requiredPermission)
        {
            _requiredPermission = requiredPermission;
        }

        public void OnAuthorization(
            AuthorizationFilterContext context)
        {
            // Not authenticated at all
            if (context.HttpContext.User.Identity?
                    .IsAuthenticated != true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Read "Permission" claims from JWT
            // Matches exactly what TokenService.cs writes:
            // claims.AddRange(permissions.Select(p =>
            //     new Claim("Permission", p)));
            var permissions = context.HttpContext.User
                .Claims
                .Where(c => c.Type == "Permission")
                .Select(c => c.Value)
                .ToList();

            if (!permissions.Contains(_requiredPermission))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}