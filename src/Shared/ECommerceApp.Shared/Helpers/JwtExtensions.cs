using System.Security.Claims;

namespace ECommerceApp.Shared.Helpers
{
    public static class JwtExtensions
    {
        // ── Maps to: ClaimTypes.NameIdentifier → user.Id ──────
        public static string GetUserId(
            this ClaimsPrincipal user)
        {
            return user.FindFirstValue(
                       ClaimTypes.NameIdentifier)
                   ?? throw new UnauthorizedAccessException(
                       "User ID claim not found in token.");
        }

        // ── Maps to: ClaimTypes.Email → user.Email ────────────
        public static string GetEmail(
            this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.Email)
                   ?? string.Empty;
        }

        // ── Maps to: ClaimTypes.Name → user.UserName ──────────
        public static string GetUserName(
            this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.Name)
                   ?? string.Empty;
        }

        // ── Maps to: custom "Name" claim → user.Name ──────────
        // (added in ApplicationUserClaimsPrincipalFactory)
        public static string GetDisplayName(
            this ClaimsPrincipal user)
        {
            return user.FindFirstValue("Name")
                   ?? string.Empty;
        }

        // ── Maps to: ClaimTypes.Role → roles (multiple) ───────
        public static IEnumerable<string> GetRoles(
            this ClaimsPrincipal user)
        {
            return user.FindAll(ClaimTypes.Role)
                       .Select(c => c.Value);
        }

        public static string GetPrimaryRole(
            this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.Role)
                   ?? string.Empty;
        }

        // ── Maps to: custom "IsAdmin" claim ───────────────────
        // (added in ApplicationUserClaimsPrincipalFactory)
        public static bool GetIsAdmin(
            this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue("IsAdmin");
            return bool.TryParse(value, out var result)
                   && result;
        }

        // ── Maps to: custom "Permission" claims (multiple) ────
        // (enforced by HasPermissionAttribute)
        public static IEnumerable<string> GetPermissions(
            this ClaimsPrincipal user)
        {
            return user.FindAll("Permission")
                       .Select(c => c.Value);
        }

        public static bool HasPermission(
            this ClaimsPrincipal user,
            string permission)
        {
            return user.FindAll("Permission")
                       .Any(c => c.Value == permission);
        }

        // ── Convenience role checks ───────────────────────────
        public static bool IsAdmin(
            this ClaimsPrincipal user)
        {
            // Check both role claim AND IsAdmin custom claim
            return user.IsInRole("admin")
                   || user.GetIsAdmin();
        }

        public static bool IsCustomer(
            this ClaimsPrincipal user)
        {
            return user.IsInRole("customer");
        }

        // ── Check if user owns a resource ─────────────────────
        public static bool IsOwnerOrAdmin(
            this ClaimsPrincipal user,
            string resourceUserId)
        {
            return user.GetUserId() == resourceUserId
                   || user.IsAdmin();
        }
    }
}