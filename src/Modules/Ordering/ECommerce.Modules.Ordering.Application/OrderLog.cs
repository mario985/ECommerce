using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Ordering.Application;

internal static partial class OrderLog
{
    [LoggerMessage(2001, LogLevel.Information,
        "Received checkout {CheckoutId} for Customer {CustomerId}")]
    public static partial void CheckoutReceived(ILogger logger, Guid checkoutId, Guid customerId);

    [LoggerMessage(2002, LogLevel.Information,
        "Ignored duplicate checkout {CheckoutId} for Customer {CustomerId}")]
    public static partial void DuplicateCheckoutIgnored(ILogger logger, Guid checkoutId, Guid customerId);

    [LoggerMessage(2003, LogLevel.Warning,
        "Rejected invalid checkout {CheckoutId} for Customer {CustomerId}")]
    public static partial void InvalidCheckoutRejected(ILogger logger, Guid checkoutId, Guid customerId);

    [LoggerMessage(2004, LogLevel.Information,
        "Created Order {OrderId} from Checkout {CheckoutId} for Customer {CustomerId}")]
    public static partial void Created(ILogger logger, Guid orderId, Guid checkoutId, Guid customerId);

    [LoggerMessage(2005, LogLevel.Information,
        "Requested Inventory reservation for Order {OrderId}, Checkout {CheckoutId}, Customer {CustomerId}")]
    public static partial void InventoryRequested(ILogger logger, Guid orderId, Guid checkoutId, Guid customerId);

    [LoggerMessage(2006, LogLevel.Information,
        "Order {OrderId} from Checkout {CheckoutId} for Customer {CustomerId} is awaiting Payment")]
    public static partial void AwaitingPayment(ILogger logger, Guid orderId, Guid checkoutId, Guid customerId);

    [LoggerMessage(2007, LogLevel.Warning,
        "Inventory reservation failed for Order {OrderId}, Checkout {CheckoutId}, Customer {CustomerId}, Product {ProductId}: {Reason}")]
    public static partial void InventoryFailed(
        ILogger logger,
        Guid orderId,
        Guid checkoutId,
        Guid customerId,
        Guid? productId,
        string reason);

    [LoggerMessage(2008, LogLevel.Warning,
        "Ignored Inventory outcome for missing Order {OrderId}, Checkout {CheckoutId}")]
    public static partial void OutcomeOrderMissing(ILogger logger, Guid orderId, Guid checkoutId);

    [LoggerMessage(2009, LogLevel.Information,
        "Ignored duplicate or invalid Inventory outcome for Order {OrderId}, Checkout {CheckoutId}, Status {Status}")]
    public static partial void OutcomeIgnored(ILogger logger, Guid orderId, Guid checkoutId, string status);

    [LoggerMessage(2010, LogLevel.Warning,
        "Payment outcome for Payment {PaymentId} referenced missing or mismatched Order {OrderId}")]
    public static partial void PaymentOutcomeOrderMissing(
        ILogger logger, Guid orderId, Guid paymentId);

    [LoggerMessage(2011, LogLevel.Information,
        "Ignored duplicate or stale Payment outcome for Order {OrderId}, Checkout {CheckoutId}, Status {Status}")]
    public static partial void PaymentOutcomeIgnored(
        ILogger logger, Guid orderId, Guid checkoutId, string status);

    [LoggerMessage(2012, LogLevel.Information,
        "Order {OrderId}, Checkout {CheckoutId} marked Paid by Payment {PaymentId}")]
    public static partial void MarkedPaid(
        ILogger logger, Guid orderId, Guid checkoutId, Guid paymentId);

    [LoggerMessage(2013, LogLevel.Warning,
        "Order {OrderId}, Checkout {CheckoutId} marked PaymentFailed by Payment {PaymentId}")]
    public static partial void PaymentFailed(
        ILogger logger, Guid orderId, Guid checkoutId, Guid paymentId);
}
