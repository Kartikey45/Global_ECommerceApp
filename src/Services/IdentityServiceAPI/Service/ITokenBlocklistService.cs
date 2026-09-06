namespace IdentityServiceAPI.Service
{
    public interface ITokenBlocklistService
    {
        /// <summary>
        /// Records a token's jti so it is rejected on subsequent requests.
        /// Safe to call twice for the same jti.
        /// </summary>
        Task RevokeAsync(string jti, string? userId, DateTime expiresAt);

        Task<bool> IsRevokedAsync(string jti);
    }
}
