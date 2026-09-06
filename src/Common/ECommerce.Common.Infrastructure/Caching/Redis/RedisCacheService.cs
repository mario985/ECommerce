using System.Text.Json;
using ECommerce.Common.Application.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace ECommerce.Common.Infrastructure.Caching.Redis;

internal sealed partial class RedisCacheService(
    IDistributedCache distributedCache,
    ILogger<RedisCacheService> logger) : ICacheService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            byte[]? value = await distributedCache.GetAsync(key, cancellationToken);
            if (value is null)
            {
                CacheMiss(logger, key);
                return default;
            }

            T? result = JsonSerializer.Deserialize<T>(value, SerializerOptions);
            CacheHit(logger, key);
            return result;
        }
        catch (JsonException exception)
        {
            DeserializationFailed(logger, key, exception.GetType().Name);
            return default;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            CacheUnavailable(logger, key, "get", exception.GetType().Name);
            return default;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        CacheEntryOptions options,
        CancellationToken cancellationToken = default)
    {
        try
        {
            byte[] payload = JsonSerializer.SerializeToUtf8Bytes(value, SerializerOptions);
            DistributedCacheEntryOptions distributedOptions = new();
            if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                distributedOptions.SetAbsoluteExpiration(options.AbsoluteExpirationRelativeToNow.Value);
            }

            await distributedCache.SetAsync(key, payload, distributedOptions, cancellationToken);
            CacheSet(logger, key);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            CacheUnavailable(logger, key, "set", exception.GetType().Name);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await distributedCache.RemoveAsync(key, cancellationToken);
            CacheRemoved(logger, key);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            CacheUnavailable(logger, key, "remove", exception.GetType().Name);
        }
    }

    [LoggerMessage(8001, LogLevel.Debug, "Cache hit for key {CacheKey}")]
    private static partial void CacheHit(ILogger logger, string cacheKey);

    [LoggerMessage(8002, LogLevel.Debug, "Cache miss for key {CacheKey}")]
    private static partial void CacheMiss(ILogger logger, string cacheKey);

    [LoggerMessage(8003, LogLevel.Debug, "Cache set for key {CacheKey}")]
    private static partial void CacheSet(ILogger logger, string cacheKey);

    [LoggerMessage(8004, LogLevel.Debug, "Cache invalidated for key {CacheKey}")]
    private static partial void CacheRemoved(ILogger logger, string cacheKey);

    [LoggerMessage(8005, LogLevel.Warning,
        "Cache operation {Operation} failed for key {CacheKey} with {FailureType}; continuing without Redis")]
    private static partial void CacheUnavailable(
        ILogger logger, string cacheKey, string operation, string failureType);

    [LoggerMessage(8006, LogLevel.Warning,
        "Cache deserialization failed for key {CacheKey} with {FailureType}; treating entry as a miss")]
    private static partial void DeserializationFailed(
        ILogger logger, string cacheKey, string failureType);
}
