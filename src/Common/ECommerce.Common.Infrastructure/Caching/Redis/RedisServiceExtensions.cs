using ECommerce.Common.Application.Caching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ECommerce.Common.Infrastructure.Caching.Redis;

internal static class RedisServiceExtensions
{
    public static IServiceCollection AddRedisCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<RedisOptions>()
            .Bind(configuration.GetSection(RedisOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString),
                "Redis:ConnectionString is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.InstanceName),
                "Redis:InstanceName is required.")
            .ValidateOnStart();

        services.AddStackExchangeRedisCache(options =>
        {
            RedisOptions redisOptions = configuration
                .GetSection(RedisOptions.SectionName)
                .Get<RedisOptions>() ?? new RedisOptions();
            options.Configuration = redisOptions.ConnectionString;
            options.InstanceName = redisOptions.InstanceName;
        });
        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddSingleton<RedisHealthCheck>();
        services.AddHealthChecks().AddCheck<RedisHealthCheck>(
            "redis",
            failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded,
            tags: ["ready", "cache"],
            timeout: TimeSpan.FromSeconds(2));
        return services;
    }
}
