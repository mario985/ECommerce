using ECommerce.Modules.Payments.Application.Abstractions;
using ECommerce.Modules.Payments.Infrastructure.Persistence.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Payments.Infrastructure.Persistence.Repositories;

internal sealed class WebhookEventRepository(PaymentsDbContext dbContext)
    : IWebhookEventRepository
{
    public Task<WebhookDeliveryStatus?> GetStatusAsync(
        string stripeEventId,
        CancellationToken cancellationToken) =>
        dbContext.ProcessedWebhookEvents
            .Where(webhookEvent => webhookEvent.StripeEventId == stripeEventId)
            .Select(webhookEvent => (WebhookDeliveryStatus?)webhookEvent.Status)
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<bool> TryAddProcessingAsync(
        PaymentWebhookEvent webhookEvent,
        DateTimeOffset receivedAtUtc,
        CancellationToken cancellationToken)
    {
        ProcessedWebhookEvent delivery = new(
            Guid.NewGuid(),
            webhookEvent.EventId,
            webhookEvent.EventType,
            webhookEvent.ProviderPaymentIntentId,
            receivedAtUtc);
        dbContext.ProcessedWebhookEvents.Add(delivery);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is SqliteException { SqliteExtendedErrorCode: 2067 })
        {
            dbContext.Entry(delivery).State = EntityState.Detached;
            return false;
        }
    }

    public Task MarkProcessedAsync(
        string stripeEventId,
        DateTimeOffset processedAtUtc,
        CancellationToken cancellationToken) =>
        UpdateAsync(
            stripeEventId,
            delivery => delivery.MarkProcessed(processedAtUtc),
            cancellationToken);

    public Task MarkIgnoredAsync(
        string stripeEventId,
        DateTimeOffset processedAtUtc,
        CancellationToken cancellationToken) =>
        UpdateAsync(
            stripeEventId,
            delivery => delivery.MarkIgnored(processedAtUtc),
            cancellationToken);

    public Task MarkFailedAsync(
        string stripeEventId,
        string safeError,
        DateTimeOffset failedAtUtc,
        CancellationToken cancellationToken) =>
        UpdateAsync(
            stripeEventId,
            delivery => delivery.MarkFailed(safeError, failedAtUtc),
            cancellationToken);

    private async Task UpdateAsync(
        string stripeEventId,
        Action<ProcessedWebhookEvent> update,
        CancellationToken cancellationToken)
    {
        ProcessedWebhookEvent delivery = await dbContext.ProcessedWebhookEvents
            .SingleAsync(
                webhookEvent => webhookEvent.StripeEventId == stripeEventId,
                cancellationToken);
        update(delivery);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
