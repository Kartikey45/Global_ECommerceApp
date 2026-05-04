using Microsoft.EntityFrameworkCore;

namespace IdentityServiceAPI.Models.EntityModels
{
    [PrimaryKey(nameof(RoleId), nameof(PermissionId))]
    public class RolePermissions
    {
        public string RoleId { get; set; }

        public string PermissionId { get; set; }

    }
}
