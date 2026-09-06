using IdentityServiceAPI.Data;
using IdentityServiceAPI.Models.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace IdentityServiceAPI.Service
{
    public class TokenBlocklistService : ITokenBlocklistService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<TokenBlocklistService> _logger;

        public TokenBlocklistService(
            ApplicationDbContext dbContext,
            ILogger<TokenBlocklistService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task RevokeAsync(
            string jti, string? userId, DateTime expiresAt)
        {
            // Logging out twice with the same token is not an error.
            var alreadyRevoked = await _dbContext.RevokedTokens
                .AnyAsync(t => t.Jti == jti);

            if (alreadyRevoked)
            {
                _logger.LogInformation(
                    "Token {Jti} was already revoked.", jti);
                return;
            }

            _dbContext.RevokedTokens.Add(new RevokedToken
            {
                Jti = jti,
                UserId = userId,
                ExpiresAt = expiresAt,
                RevokedAt = DateTime.UtcNow
            });

            // Drop rows whose tokens have expired on their own —
            // keeps the table from growing without a background job.
            var stale = await _dbContext.RevokedTokens
                .Where(t => t.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            if (stale.Count > 0)
                _dbContext.RevokedTokens.RemoveRange(stale);

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Token {Jti} revoked for user {UserId}. " +
                "Purged {Count} expired entries.",
                jti, userId, stale.Count);
        }

        public async Task<bool> IsRevokedAsync(string jti)
            => await _dbContext.RevokedTokens
                .AnyAsync(t => t.Jti == jti);
    }
}
