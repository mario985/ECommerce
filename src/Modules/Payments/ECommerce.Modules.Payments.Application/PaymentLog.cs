using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Payments.Application;

internal static partial class PaymentLog
{
    [LoggerMessage(3001, LogLevel.Information,
        "Received Order {OrderId} awaiting payment for Customer {CustomerId}")]
    public static partial void OrderAwaitingPaymentReceived(
        ILogger logger, Guid orderId, Guid customerId);

    [LoggerMessage(3002, LogLevel.Information,
        "Created Payment {PaymentId} for Order {OrderId} and Customer {CustomerId}")]
    public static partial void Created(
        ILogger logger, Guid paymentId, Guid orderId, Guid customerId);

    [LoggerMessage(3003, LogLevel.Information,
        "Ignored duplicate payment request for Payment {PaymentId}, Order {OrderId}, Customer {CustomerId}")]
    public static partial void DuplicateIgnored(
        ILogger logger, Guid paymentId, Guid orderId, Guid customerId);

    [LoggerMessage(3004, LogLevel.Information,
        "Starting Stripe PaymentIntent creation for Payment {PaymentId}, Order {OrderId}, Customer {CustomerId}")]
    public static partial void IntentCreationStarted(
        ILogger logger, Guid paymentId, Guid orderId, Guid customerId);

    [LoggerMessage(3005, LogLevel.Information,
        "Created Stripe PaymentIntent {ProviderPaymentIntentId} for Payment {PaymentId}, Order {OrderId}, Customer {CustomerId}")]
    public static partial void IntentCreated(
        ILogger logger,
        string providerPaymentIntentId,
        Guid paymentId,
        Guid orderId,
        Guid customerId);

    [LoggerMessage(3006, LogLevel.Warning,
        "Stripe PaymentIntent creation failed with {FailureCode} for Payment {PaymentId}, Order {OrderId}, Customer {CustomerId}")]
    public static partial void IntentCreationFailed(
        ILogger logger,
        string failureCode,
        Guid paymentId,
        Guid orderId,
        Guid customerId);

    [LoggerMessage(3007, LogLevel.Warning,
        "Rejected invalid awaiting-payment event for Order {OrderId}, Customer {CustomerId}")]
    public static partial void InvalidEventRejected(
        ILogger logger, Guid orderId, Guid customerId);
}
