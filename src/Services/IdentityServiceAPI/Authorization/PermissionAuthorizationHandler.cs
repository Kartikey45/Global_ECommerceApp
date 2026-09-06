using Microsoft.AspNetCore.Authorization;

namespace IdentityServiceAPI.Authorization
{
    /// <summary>
    /// Decides whether the caller holds the required permission.
    ///
    /// Permissions are read from the JWT's "Permission" claims, which
    /// TokenService populated at login from the user's roles via the
    /// RolePermissions table. No database call is needed here — the
    /// token already carries the answer.
    ///
    /// Consequence worth knowing: changing a role's permissions in the
    /// DB does not affect tokens that were already issued. The user
    /// must log in again (or be logged out) to pick up the change.
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

                // Not calling Fail() — just not succeeding is enough,
                // and it lets other handlers/policies still grant access.
            }

            return Task.CompletedTask;
        }
    }
}
