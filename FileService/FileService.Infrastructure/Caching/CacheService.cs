using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using FileService.Application.Interfaces;

namespace FileService.Infrastructure.Caching;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CacheService> _logger;

    public CacheService(
        IDistributedCache distributedCache,
        IMemoryCache memoryCache,
        ILogger<CacheService> logger)
    {
        _distributedCache = distributedCache;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        // Try memory cache first (L1)
        if (_memoryCache.TryGetValue(key, out T? cachedValue))
        {
            _logger.LogDebug("Cache hit in memory cache for key: {Key}", key);
            return cachedValue;
        }

        // Try distributed cache (L2)
        var distributedValue = await _distributedCache.GetStringAsync(key, cancellationToken);
        if (!string.IsNullOrEmpty(distributedValue))
        {
            _logger.LogDebug("Cache hit in distributed cache for key: {Key}", key);
            var value = JsonSerializer.Deserialize<T>(distributedValue);
            
            // Store in memory cache for faster subsequent access
            _memoryCache.Set(key, value, TimeSpan.FromMinutes(5));
            
            return value;
        }

        _logger.LogDebug("Cache miss for key: {Key}", key);
        return null;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var expirationTime = expiration ?? TimeSpan.FromHours(1);
        
        // Set in memory cache (L1)
        _memoryCache.Set(key, value, TimeSpan.FromMinutes(5));

        // Set in distributed cache (L2)
        var serializedValue = JsonSerializer.Serialize(value);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expirationTime
        };

        await _distributedCache.SetStringAsync(key, serializedValue, options, cancellationToken);
        _logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}", key, expirationTime);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(key);
        await _distributedCache.RemoveAsync(key, cancellationToken);
        _logger.LogDebug("Removed cache for key: {Key}", key);
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var cachedValue = await GetAsync<T>(key, cancellationToken);
        if (cachedValue != null)
        {
            return cachedValue;
        }

        var value = await factory();
        await SetAsync(key, value, expiration, cancellationToken);
        return value;
    }
}
