namespace ECommerce.Modules.Ordering.Domain.Shipments;

public enum ShipmentStatus
{
    Pending = 1,
    Preparing = 2,
    Shipped = 3,
    OutForDelivery = 4,
    Delivered = 5,
    Cancelled = 6,
}
