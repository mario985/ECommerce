using ECommerce.Modules.Payments.Application.Abstractions;

namespace ECommerce.Modules.Payments.Infrastructure.Persistence.Models;

public sealed class ProcessedWebhookEvent
{
    private ProcessedWebhookEvent()
    {
        StripeEventId = string.Empty;
        EventType = string.Empty;
        ProviderPaymentIntentId = string.Empty;
    }

    public ProcessedWebhookEvent(
        Guid id,
        string stripeEventId,
        string eventType,
        string providerPaymentIntentId,
        DateTimeOffset receivedAtUtc)
    {
        Id = id;
        StripeEventId = stripeEventId;
        EventType = eventType;
        ProviderPaymentIntentId = providerPaymentIntentId;
        ReceivedAtUtc = receivedAtUtc;
        Status = WebhookDeliveryStatus.Processing;
    }

    public Guid Id { get; private set; }
    public string StripeEventId { get; private set; }
    public string EventType { get; private set; }
    public string ProviderPaymentIntentId { get; private set; }
    public DateTimeOffset ReceivedAtUtc { get; private set; }
    public DateTimeOffset? ProcessedAtUtc { get; private set; }
    public WebhookDeliveryStatus Status { get; private set; }
    public string? LastError { get; private set; }

    public void MarkProcessed(DateTimeOffset processedAtUtc)
    {
        Status = WebhookDeliveryStatus.Processed;
        ProcessedAtUtc = processedAtUtc;
        LastError = null;
    }

    public void MarkIgnored(DateTimeOffset processedAtUtc)
    {
        Status = WebhookDeliveryStatus.Ignored;
        ProcessedAtUtc = processedAtUtc;
        LastError = null;
    }

    public void MarkFailed(string safeError, DateTimeOffset failedAtUtc)
    {
        Status = WebhookDeliveryStatus.Failed;
        ProcessedAtUtc = failedAtUtc;
        string trimmed = safeError.Trim();
        LastError = trimmed.Length <= 500 ? trimmed : trimmed[..500];
    }
}
