using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace ECommerceApp.Shared.Authorization
{
    /// <summary>
    /// Decides whether the caller holds the required permission.
    ///
    /// Permissions come from the JWT's "Permission" claims, which the
    /// Identity service populated at login from the user's roles via
    /// the RolePermissions table. No database call happens here — the
    /// token already carries the answer, which is what lets every
    /// service enforce permissions without reaching into
    /// management_system.
    ///
    /// Consequence: changing a role's permissions in the DB does not
    /// affect tokens already issued. The user must log in again to
    /// pick up the change.
    /// </summary>
    public class PermissionAuthorizationHandler
        : AuthorizationHandler<PermissionRequirement>
    {
        private readonly ILogger<PermissionAuthorizationHandler> _logger;

        public PermissionAuthorizationHandler(
            ILogger<PermissionAuthorizationHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                // Leave unhandled — the framework returns 401.
                return Task.CompletedTask;
            }

            var granted = context.User.Claims
                .Where(c => c.Type == Permission.ClaimType)
                .Select(c => c.Value)
                .ToList();

            if (granted.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning(
                    "Permission denied. Required: {Required}. " +
                    "Token holds: [{Granted}].",
                    requirement.Permission,
                    string.Join(", ", granted));

                // Not calling Fail() — simply not succeeding is enough,
                // and it lets other policies still grant access.
            }

            return Task.CompletedTask;
        }
    }
}
