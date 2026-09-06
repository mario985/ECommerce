namespace ECommerce.Modules.Ordering.Application.Caching;

public static class ShipmentTrackingCacheKeys
{
    public static string ForOrder(Guid orderId) => $"ordering:tracking:{orderId:D}";
}
