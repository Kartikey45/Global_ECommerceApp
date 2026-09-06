using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace IdentityServiceAPI.Authorization
{
    /// <summary>
    /// Builds an authorization policy on demand for any policy name
    /// shaped "Permission:{name}".
    ///
    /// Without this, every permission would need an explicit
    /// AddPolicy(...) call at startup. This lets [HasPermission("X")]
    /// work for any X — including permissions added to the DB later —
    /// with no startup change.
    /// </summary>
    public class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        public const string Prefix = "Permission:";

        // Handles anything that is not a permission policy
        // (e.g. [Authorize(Roles = "...")] and plain [Authorize]).
        private readonly DefaultAuthorizationPolicyProvider _fallback;

        public PermissionPolicyProvider(
            IOptions<AuthorizationOptions> options)
        {
            _fallback = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
            => _fallback.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
            => _fallback.GetFallbackPolicyAsync();

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(
                    Prefix, StringComparison.OrdinalIgnoreCase))
            {
                var permission = policyName.Substring(Prefix.Length);

                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new PermissionRequirement(permission))
                    .Build();

                return Task.FromResult<AuthorizationPolicy?>(policy);
            }

            return _fallback.GetPolicyAsync(policyName);
        }
    }
}
