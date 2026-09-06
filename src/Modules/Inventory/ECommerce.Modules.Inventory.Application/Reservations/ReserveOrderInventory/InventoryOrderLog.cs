using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Inventory.Application.Reservations.ReserveOrderInventory;

internal static partial class InventoryOrderLog
{
    [LoggerMessage(3001, LogLevel.Information,
        "Received Inventory reservation request for Order {OrderId}, Checkout {CheckoutId}")]
    public static partial void ReservationReceived(ILogger logger, Guid orderId, Guid checkoutId);

    [LoggerMessage(3002, LogLevel.Information,
        "Reserved Inventory atomically for Order {OrderId}, Checkout {CheckoutId}, across {ItemCount} items")]
    public static partial void ReservationSucceeded(ILogger logger, Guid orderId, Guid checkoutId, int itemCount);

    [LoggerMessage(3003, LogLevel.Warning,
        "Inventory reservation failed for Order {OrderId}, Checkout {CheckoutId}, Product {ProductId}: {Reason}")]
    public static partial void ReservationFailed(
        ILogger logger,
        Guid orderId,
        Guid checkoutId,
        Guid? productId,
        string reason);

    [LoggerMessage(3004, LogLevel.Information,
        "Confirmed {ReservationCount} Inventory reservations for Order {OrderId}")]
    public static partial void ReservationsConfirmed(
        ILogger logger, Guid orderId, int reservationCount);

    [LoggerMessage(3005, LogLevel.Information,
        "Released {ReservationCount} Inventory reservations for Order {OrderId}")]
    public static partial void ReservationsReleased(
        ILogger logger, Guid orderId, int reservationCount);

    [LoggerMessage(3006, LogLevel.Information,
        "Ignored duplicate Inventory finalization for Order {OrderId}")]
    public static partial void FinalizationIgnored(ILogger logger, Guid orderId);
}
