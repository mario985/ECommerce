using ECommerce.Common.Application.Errors;

namespace ECommerce.Modules.Payments.Application.Abstractions;

public interface IPaymentWebhookVerifier
{
    Result<PaymentWebhookEvent> Verify(string payload, string signature);
}

public sealed record PaymentWebhookEvent(
    string EventId,
    string EventType,
    string ProviderPaymentIntentId,
    string ProviderStatus,
    string? FailureCode,
    string? FailureMessage,
    DateTimeOffset OccurredAtUtc);
