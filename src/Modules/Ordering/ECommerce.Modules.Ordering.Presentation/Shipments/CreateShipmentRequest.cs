namespace ECommerce.Modules.Ordering.Presentation.Shipments;

public sealed record CreateShipmentRequest(string Carrier, string TrackingNumber);
