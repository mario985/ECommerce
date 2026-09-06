using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Ordering.Domain.Shipments;

namespace ECommerce.Modules.Ordering.Application.Shipments;

public static class ShipmentApplicationErrors
{
    public static readonly Error NotFound = new(
        ShipmentErrors.NotFoundCode, "The shipment could not be found.", ErrorType.NotFound);
    public static readonly Error OrderNotFound = new(
        ShipmentErrors.OrderNotFoundCode, "The order could not be found.", ErrorType.NotFound);
    public static readonly Error OrderNotPaid = new(
        ShipmentErrors.OrderNotPaidCode,
        "A shipment can only be created or prepared for a paid order.",
        ErrorType.Conflict);
    public static readonly Error AlreadyExists = new(
        ShipmentErrors.AlreadyExistsCode, "The order already has a shipment.", ErrorType.Conflict);
    public static readonly Error InvalidStatusTransition = new(
        ShipmentErrors.InvalidStatusTransitionCode,
        "The shipment cannot make the requested status transition.",
        ErrorType.Conflict);
    public static readonly Error AlreadyDelivered = new(
        ShipmentErrors.AlreadyDeliveredCode, "A delivered shipment cannot be changed.", ErrorType.Conflict);
    public static readonly Error InvalidTrackingNumber = new(
        ShipmentErrors.InvalidTrackingNumberCode,
        "A tracking number between 1 and 100 characters is required.",
        ErrorType.Validation);
    public static readonly Error InvalidCarrier = new(
        ShipmentErrors.InvalidCarrierCode,
        "A carrier between 1 and 100 characters is required.",
        ErrorType.Validation);
    public static readonly Error Forbidden = new(
        ShipmentErrors.ForbiddenCode,
        "The shipment belongs to another customer's order.",
        ErrorType.Forbidden);
    public static readonly Error Unauthorized = new(
        ShipmentErrors.UnauthorizedCode,
        "Authentication is required to view order tracking.",
        ErrorType.Unauthorized);
    public static readonly Error InvalidQuery = new(
        ShipmentErrors.InvalidQueryCode,
        "Shipment query parameters are invalid.",
        ErrorType.Validation);
}
