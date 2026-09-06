namespace ECommerce.Common.Infrastructure.Caching.Redis;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; init; } = string.Empty;
    public string InstanceName { get; init; } = "ECommerce:";
}
