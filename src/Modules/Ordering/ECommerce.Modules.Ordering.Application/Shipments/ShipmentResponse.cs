using ECommerce.Modules.Ordering.Domain.Shipments;

namespace ECommerce.Modules.Ordering.Application.Shipments;

public sealed record ShipmentResponse(
    Guid Id,
    Guid OrderId,
    string Status,
    string Carrier,
    string TrackingNumber,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ShippedAtUtc,
    DateTimeOffset? DeliveredAtUtc)
{
    public static ShipmentResponse From(Shipment shipment) => new(
        shipment.Id,
        shipment.OrderId,
        shipment.Status.ToString(),
        shipment.Carrier,
        shipment.TrackingNumber,
        shipment.CreatedAtUtc,
        shipment.ShippedAtUtc,
        shipment.DeliveredAtUtc);
}
