using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using User.Domain.Interfaces.Services.Caching;

namespace User.Infrastructure.Services.Caching
{
    internal sealed class CachingService(
        IMemoryCache memoryCache,
        IDistributedCache distributedCache
    ) : ICachingService
    {
        public T? Get<T>(string key)
        {
            if (memoryCache.TryGetValue(key, out T? value))
                return value;

            var cachedData = distributedCache.GetString(key);

            if (!string.IsNullOrEmpty(cachedData))
                return JsonSerializer.Deserialize<T>(cachedData);

            return default;
        }

        public void Remove(string key)
        {
            memoryCache.Remove(key);

            distributedCache.Remove(key);
        }

        public void Set<T>(string key, T value, TimeSpan? expiration = null)
        {
            var options = new MemoryCacheEntryOptions();
            if (expiration.HasValue)
                options.SetAbsoluteExpiration(expiration.Value);

            memoryCache.Set(key, value, options);

            var serializedData = JsonSerializer.Serialize(value);
            var cacheOptions = new DistributedCacheEntryOptions();
            if (expiration.HasValue)
                cacheOptions.SetAbsoluteExpiration(expiration.Value);

            distributedCache.SetString(key, serializedData, cacheOptions);
        }
    }
}
