namespace ECommerceApp.Shared.Authorization
{
    /// <summary>
    /// Permission names as stored in the Identity service's Permissions
    /// table. A role is granted a subset of these via RolePermissions,
    /// and TokenService writes them into the JWT as "Permission" claims
    /// at login.
    ///
    /// Services other than Identity have no access to management_system,
    /// so they read permissions straight off the token — no DB call.
    /// </summary>
    public static class Permission
    {
        public const string View = "View";
        public const string Create = "Create";
        public const string Edit = "Edit";
        public const string Delete = "Delete";

        /// <summary>Claim type used for permissions in the JWT.</summary>
        public const string ClaimType = "Permission";

        public static readonly string[] All =
        {
            View, Create, Edit, Delete
        };
    }
}
