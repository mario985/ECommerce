using ECommerce.Modules.Ordering.Domain.Shipments;

namespace ECommerce.Modules.Ordering.Presentation.Shipments;

public sealed record UpdateShipmentStatusRequest(ShipmentStatus Status);
