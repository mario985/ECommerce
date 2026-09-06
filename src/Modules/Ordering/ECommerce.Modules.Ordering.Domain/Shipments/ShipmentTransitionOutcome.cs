namespace ECommerce.Modules.Ordering.Domain.Shipments;

public enum ShipmentTransitionOutcome
{
    Applied = 1,
    AlreadyApplied = 2,
    InvalidState = 3,
    AlreadyDelivered = 4,
}
