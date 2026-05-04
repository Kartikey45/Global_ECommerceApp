using ECommerceApp.Product.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;

namespace ECommerceApp.Product.Services.Implementations
{
    public class DistributedCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<DistributedCacheService> _logger;
        private readonly string _instanceName;

        public DistributedCacheService(
            IDistributedCache cache,
            IConnectionMultiplexer redis,
            IConfiguration config,
            ILogger<DistributedCacheService> logger)
        {
            _cache = cache;
            _redis = redis;
            _logger = logger;
            _instanceName =
                config["Redis:InstanceName"] ?? "product:";
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var data = await _cache.GetStringAsync(key);
                if (data is null)
                {
                    _logger.LogDebug(
                        "Cache MISS → {Key}", key);
                    return default;
                }

                _logger.LogDebug(
                    "Cache HIT  → {Key}", key);
                return JsonSerializer.Deserialize<T>(data);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Cache GET failed for {Key}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(
            string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow =
                        expiry ?? TimeSpan.FromMinutes(30)
                };
                var json = JsonSerializer.Serialize(value);
                await _cache.SetStringAsync(
                    key, json, options);

                _logger.LogDebug(
                    "Cache SET → {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Cache SET failed for {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
                _logger.LogDebug(
                    "Cache REMOVE → {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Cache REMOVE failed for {Key}", key);
            }
        }

        public async Task RemoveByPrefixAsync(string prefix)
        {
            try
            {
                var server = _redis.GetServer(
                    _redis.GetEndPoints().First());

                // Include instance name in pattern
                // because IDistributedCache prepends it
                var pattern =
                    $"{_instanceName}{prefix}*";

                var keys = server
                    .Keys(pattern: pattern)
                    .ToArray();

                if (keys.Length == 0) return;

                var db = _redis.GetDatabase();
                await db.KeyDeleteAsync(keys);

                _logger.LogDebug(
                    "Cache REMOVE PREFIX → " +
                    "{Count} keys for {Pattern}",
                    keys.Length, pattern);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Cache REMOVE PREFIX failed: {Prefix}",
                    prefix);
            }
        }

        public async Task<T> GetOrSetAsync<T>(
            string key,
            Func<Task<T>> factory,
            TimeSpan? expiry = null)
        {
            try
            {
                var cached = await GetAsync<T>(key);
                if (cached is not null)
                    return cached;

                var value = await factory();

                if (value is not null)
                    await SetAsync(key, value, expiry);

                return value;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "GetOrSet failed for {Key} — " +
                    "calling factory directly", key);
                return await factory();
            }
        }
    }
}