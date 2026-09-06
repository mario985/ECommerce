using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Cart.Application.Carts;

internal static partial class CartLog
{
    [LoggerMessage(1001, LogLevel.Information,
        "Removed Product {ProductId} from Cart {CartId} for Customer {CustomerId}")]
    public static partial void ItemRemoved(ILogger logger, Guid cartId, Guid customerId, Guid productId);

    [LoggerMessage(1002, LogLevel.Warning,
        "Rejected removal of missing Product {ProductId} for Customer {CustomerId}")]
    public static partial void ItemRemovalRejected(ILogger logger, Guid customerId, Guid productId);

    [LoggerMessage(1003, LogLevel.Information,
        "Cleared Cart {CartId} for Customer {CustomerId}")]
    public static partial void Cleared(ILogger logger, Guid cartId, Guid customerId);

    [LoggerMessage(1004, LogLevel.Debug,
        "Skipped clearing an empty or absent Cart for Customer {CustomerId}")]
    public static partial void ClearSkipped(ILogger logger, Guid customerId);

    [LoggerMessage(1005, LogLevel.Warning,
        "Rejected checkout of empty Cart {CartId} for Customer {CustomerId}")]
    public static partial void EmptyCheckoutRejected(ILogger logger, Guid cartId, Guid customerId);

    [LoggerMessage(1006, LogLevel.Warning,
        "Rejected checkout for Cart {CartId}, Customer {CustomerId}, Product {ProductId}: {Reason}")]
    public static partial void CheckoutProductRejected(
        ILogger logger,
        Guid cartId,
        Guid customerId,
        Guid productId,
        string reason);

    [LoggerMessage(1007, LogLevel.Information,
        "Requested Checkout {CheckoutId} for Cart {CartId}, Customer {CustomerId}, with {ItemCount} items")]
    public static partial void CheckoutRequested(
        ILogger logger,
        Guid checkoutId,
        Guid cartId,
        Guid customerId,
        int itemCount);

    [LoggerMessage(1008, LogLevel.Information,
        "Published checkout Event {EventId} for Checkout {CheckoutId}, Cart {CartId}, Customer {CustomerId}")]
    public static partial void CheckoutEventPublished(
        ILogger logger,
        Guid eventId,
        Guid checkoutId,
        Guid cartId,
        Guid customerId);

    [LoggerMessage(1009, LogLevel.Information,
        "Cart checkout created: Checkout {CheckoutId}, Cart {CartId}, Customer {CustomerId}, Items {ItemCount}")]
    public static partial void CheckoutCreated(
        ILogger logger, Guid checkoutId, Guid cartId, Guid customerId, int itemCount);

    [LoggerMessage(1010, LogLevel.Warning,
        "Cart checkout already pending: Checkout {CheckoutId}, Cart {CartId}, Customer {CustomerId}")]
    public static partial void CheckoutAlreadyPending(
        ILogger logger, Guid checkoutId, Guid cartId, Guid customerId);

    [LoggerMessage(1011, LogLevel.Information,
        "Order paid event received: Checkout {CheckoutId}, Order {OrderId}, Customer {CustomerId}")]
    public static partial void OrderPaidReceived(
        ILogger logger, Guid checkoutId, Guid orderId, Guid customerId);

    [LoggerMessage(1012, LogLevel.Information,
        "Cart reconciliation started: Checkout {CheckoutId}, Cart {CartId}, Order {OrderId}, Customer {CustomerId}")]
    public static partial void ReconciliationStarted(
        ILogger logger, Guid checkoutId, Guid cartId, Guid orderId, Guid customerId);

    [LoggerMessage(1013, LogLevel.Information,
        "Purchased quantity removed: Checkout {CheckoutId}, Cart {CartId}, Product {ProductId}, Quantity {Quantity}")]
    public static partial void PurchasedQuantityRemoved(
        ILogger logger, Guid checkoutId, Guid cartId, Guid productId, int quantity);

    [LoggerMessage(1014, LogLevel.Information,
        "Cart checkout completed: Checkout {CheckoutId}, Cart {CartId}, Order {OrderId}, Customer {CustomerId}")]
    public static partial void CheckoutCompleted(
        ILogger logger, Guid checkoutId, Guid cartId, Guid orderId, Guid customerId);

    [LoggerMessage(1015, LogLevel.Information,
        "Payment failure received: Checkout {CheckoutId}, Order {OrderId}, Customer {CustomerId}")]
    public static partial void PaymentFailureReceived(
        ILogger logger, Guid checkoutId, Guid orderId, Guid customerId);

    [LoggerMessage(1016, LogLevel.Information,
        "Cart checkout failed: Checkout {CheckoutId}, Cart {CartId}, Order {OrderId}, Customer {CustomerId}")]
    public static partial void CheckoutFailed(
        ILogger logger, Guid checkoutId, Guid cartId, Guid orderId, Guid customerId);

    [LoggerMessage(1017, LogLevel.Information,
        "Duplicate checkout finalization ignored: Checkout {CheckoutId}, Order {OrderId}, Status {Status}")]
    public static partial void DuplicateFinalizationIgnored(
        ILogger logger, Guid checkoutId, Guid orderId, string status);

    [LoggerMessage(1018, LogLevel.Warning,
        "Checkout finalization ignored because checkout was not found: Checkout {CheckoutId}, Order {OrderId}, Customer {CustomerId}")]
    public static partial void FinalizationCheckoutMissing(
        ILogger logger, Guid checkoutId, Guid orderId, Guid customerId);
}
