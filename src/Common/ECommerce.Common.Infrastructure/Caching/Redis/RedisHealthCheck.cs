using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ECommerce.Common.Infrastructure.Caching.Redis;

internal sealed class RedisHealthCheck(IOptions<RedisOptions> options) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            ConfigurationOptions configuration = ConfigurationOptions.Parse(options.Value.ConnectionString);
            configuration.AbortOnConnectFail = false;
            await using ConnectionMultiplexer connection = await ConnectionMultiplexer.ConnectAsync(configuration);
            TimeSpan latency = await connection.GetDatabase().PingAsync();
            return HealthCheckResult.Healthy("Redis responded.", new Dictionary<string, object>
            {
                ["latencyMs"] = latency.TotalMilliseconds,
            });
        }
        catch (Exception)
        {
            return HealthCheckResult.Degraded(
                "Redis cache is unavailable; cache-aside fallback remains active.");
        }
    }
}
