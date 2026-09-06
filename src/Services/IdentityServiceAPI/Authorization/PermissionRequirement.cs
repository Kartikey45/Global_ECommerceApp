using Microsoft.AspNetCore.Authorization;

namespace IdentityServiceAPI.Authorization
{
    /// <summary>
    /// Requires the caller's token to carry a given permission.
    /// </summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }
}
