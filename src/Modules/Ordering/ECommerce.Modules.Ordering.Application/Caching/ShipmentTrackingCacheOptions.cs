namespace ECommerce.Modules.Ordering.Application.Caching;

public sealed class ShipmentTrackingCacheOptions
{
    public const string SectionName = "ShipmentTrackingCache";

    public int ExpirationMinutes { get; init; } = 5;

    public TimeSpan Expiration => TimeSpan.FromMinutes(ExpirationMinutes);
}
