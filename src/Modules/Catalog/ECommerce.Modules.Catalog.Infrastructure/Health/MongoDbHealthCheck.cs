using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ECommerce.Modules.Catalog.Infrastructure.Health;

internal sealed class MongoDbHealthCheck(IMongoDatabase database) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await database.RunCommandAsync<BsonDocument>(
                new BsonDocument("ping", 1),
                cancellationToken: cancellationToken);
            return HealthCheckResult.Healthy("Catalog MongoDB responded.");
        }
        catch (Exception)
        {
            return HealthCheckResult.Unhealthy("Catalog MongoDB is unavailable.");
        }
    }
}
