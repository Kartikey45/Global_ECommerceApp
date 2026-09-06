namespace IdentityServiceAPI.Authorization
{
    /// <summary>
    /// The permission names stored in the Permissions table.
    /// A role is granted a subset of these via RolePermissions,
    /// and they are written into the JWT as "Permission" claims
    /// at login (see TokenService.GenerateToken).
    ///
    /// Use these constants instead of string literals so a typo
    /// is a compile error rather than a silent 403.
    /// </summary>
    public static class Permission
    {
        public const string View = "View";
        public const string Create = "Create";
        public const string Edit = "Edit";
        public const string Delete = "Delete";

        /// <summary>The claim type used for permissions in the JWT.</summary>
        public const string ClaimType = "Permission";

        public static readonly string[] All =
        {
            View, Create, Edit, Delete
        };
    }
}
