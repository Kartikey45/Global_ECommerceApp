namespace IdentityServiceAPI.Authorization
{
    /// <summary>
    /// Role names as stored in AspNetRoles.
    ///
    /// These strings must match the database EXACTLY — role checks are
    /// case-sensitive and space-sensitive, so "SuperAdmin" will not
    /// match the "Super Admin" row and every request would 403.
    ///
    /// Use the Groups constants in [Authorize(Roles = ...)] so the
    /// comma-separated lists stay consistent across endpoints.
    /// </summary>
    public static class Role
    {
        public const string SuperAdmin = "Super Admin";
        public const string Admin = "Admin";
        public const string Customer = "Customer";

        /// <summary>
        /// Pre-built role lists for [Authorize(Roles = ...)].
        /// Comma-separated means OR — any one role is enough.
        /// Must be const (not static readonly) because attribute
        /// arguments have to be compile-time constants.
        /// </summary>
        public static class Groups
        {
            /// <summary>Highest privilege only.</summary>
            public const string SuperAdminOnly = SuperAdmin;

            /// <summary>Any administrative role.</summary>
            public const string Admins = SuperAdmin + "," + Admin;

            /// <summary>Every signed-in role — admins and customers.</summary>
            public const string All =
                SuperAdmin + "," + Admin + "," + Customer;
        }
    }
}
