using ECommerce.Modules.Ordering.Domain.Shipments;

namespace ECommerce.Modules.Ordering.Application.Shipments.GetOrderTracking;

public sealed record OrderTrackingResponse(
    Guid OrderId,
    string Status,
    string Carrier,
    string TrackingNumber,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ShippedAtUtc,
    DateTimeOffset? DeliveredAtUtc)
{
    public static OrderTrackingResponse From(Shipment shipment) => new(
        shipment.OrderId,
        shipment.Status.ToString(),
        shipment.Carrier,
        shipment.TrackingNumber,
        shipment.CreatedAtUtc,
        shipment.ShippedAtUtc,
        shipment.DeliveredAtUtc);
}
