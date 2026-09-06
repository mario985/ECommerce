namespace ECommerce.Modules.Payments.Application.Abstractions;

public interface IWebhookEventRepository
{
    Task<WebhookDeliveryStatus?> GetStatusAsync(
        string stripeEventId,
        CancellationToken cancellationToken);

    Task<bool> TryAddProcessingAsync(
        PaymentWebhookEvent webhookEvent,
        DateTimeOffset receivedAtUtc,
        CancellationToken cancellationToken);

    Task MarkProcessedAsync(
        string stripeEventId,
        DateTimeOffset processedAtUtc,
        CancellationToken cancellationToken);

    Task MarkIgnoredAsync(
        string stripeEventId,
        DateTimeOffset processedAtUtc,
        CancellationToken cancellationToken);

    Task MarkFailedAsync(
        string stripeEventId,
        string safeError,
        DateTimeOffset failedAtUtc,
        CancellationToken cancellationToken);
}

public enum WebhookDeliveryStatus
{
    Processing = 1,
    Processed = 2,
    Failed = 3,
    Ignored = 4,
}
