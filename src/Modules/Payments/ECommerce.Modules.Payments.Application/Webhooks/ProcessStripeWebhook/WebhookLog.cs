using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Payments.Application.Webhooks.ProcessStripeWebhook;

internal static partial class WebhookLog
{
    [LoggerMessage(3201, LogLevel.Information, "Stripe webhook received")]
    public static partial void Received(ILogger logger);

    [LoggerMessage(3202, LogLevel.Information,
        "Stripe webhook {StripeEventId} verified for PaymentIntent {ProviderPaymentIntentId}")]
    public static partial void Verified(
        ILogger logger, string stripeEventId, string providerPaymentIntentId);

    [LoggerMessage(3203, LogLevel.Information,
        "Duplicate Stripe webhook {StripeEventId} skipped")]
    public static partial void DuplicateSkipped(ILogger logger, string stripeEventId);

    [LoggerMessage(3204, LogLevel.Information,
        "PaymentIntent {ProviderPaymentIntentId} is processing for Payment {PaymentId}, Order {OrderId}")]
    public static partial void Processing(
        ILogger logger, string providerPaymentIntentId, Guid paymentId, Guid orderId);

    [LoggerMessage(3205, LogLevel.Information,
        "Payment {PaymentId} for Order {OrderId} succeeded from Stripe event {StripeEventId}")]
    public static partial void Succeeded(
        ILogger logger, Guid paymentId, Guid orderId, string stripeEventId);

    [LoggerMessage(3206, LogLevel.Warning,
        "Payment {PaymentId} for Order {OrderId} failed from Stripe event {StripeEventId} with code {FailureCode}")]
    public static partial void Failed(
        ILogger logger, Guid paymentId, Guid orderId, string stripeEventId, string? failureCode);

    [LoggerMessage(3207, LogLevel.Information,
        "Stripe webhook {StripeEventId} of type {EventType} was ignored")]
    public static partial void Ignored(ILogger logger, string stripeEventId, string eventType);

    [LoggerMessage(3208, LogLevel.Warning,
        "Stripe webhook {StripeEventId} referenced unknown PaymentIntent {ProviderPaymentIntentId}")]
    public static partial void UnknownPayment(
        ILogger logger, string stripeEventId, string providerPaymentIntentId);

    [LoggerMessage(3209, LogLevel.Error,
        "Stripe webhook {StripeEventId} failed during processing")]
    public static partial void ProcessingFailed(
        ILogger logger, string stripeEventId, Exception exception);

    [LoggerMessage(3210, LogLevel.Information,
        "Stripe webhook {StripeEventId} recovered business correlation for Payment {PaymentId}, Order {OrderId}")]
    public static partial void BusinessCorrelationRecovered(
        ILogger logger, string stripeEventId, Guid paymentId, Guid orderId);
}
