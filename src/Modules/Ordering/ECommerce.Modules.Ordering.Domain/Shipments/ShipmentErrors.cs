namespace ECommerce.Modules.Ordering.Domain.Shipments;

public static class ShipmentErrors
{
    public const string NotFoundCode = "Shipment.NotFound";
    public const string InvalidStatusTransitionCode = "Shipment.InvalidStatusTransition";
    public const string OrderNotPaidCode = "Shipment.OrderNotPaid";
    public const string AlreadyExistsCode = "Shipment.AlreadyExists";
    public const string AlreadyDeliveredCode = "Shipment.AlreadyDelivered";
    public const string InvalidTrackingNumberCode = "Shipment.InvalidTrackingNumber";
    public const string OrderNotFoundCode = "Shipment.OrderNotFound";
    public const string InvalidCarrierCode = "Shipment.InvalidCarrier";
    public const string ForbiddenCode = "Shipment.Forbidden";
    public const string UnauthorizedCode = "Shipment.Unauthorized";
    public const string InvalidQueryCode = "Shipment.InvalidQuery";
}
