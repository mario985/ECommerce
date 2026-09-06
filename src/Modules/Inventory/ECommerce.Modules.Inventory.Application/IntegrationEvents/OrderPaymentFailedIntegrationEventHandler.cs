using ECommerce.Modules.Inventory.Application.Reservations.ReserveOrderInventory;
using ECommerce.Common.Application.Messaging;
using ECommerce.Common.Application.Observability;
using ECommerce.Modules.Inventory.Contracts.IntegrationEvents;
using ECommerce.Modules.Inventory.Domain.StockItems;
using ECommerce.Modules.Ordering.Contracts.IntegrationEvents;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Inventory.Application.IntegrationEvents;

#pragma warning disable CA1711 // Integration event handler is the architecture's explicit terminology.
public sealed class OrderPaymentFailedIntegrationEventHandler(
    IStockItemRepository stockItemRepository,
    IIntegrationEventPublisher integrationEventPublisher,
    ILogger<OrderPaymentFailedIntegrationEventHandler> logger)
    : INotificationHandler<OrderPaymentFailedIntegrationEvent>
{
    public async Task Handle(
        OrderPaymentFailedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        using System.Diagnostics.Activity? activity = CommerceActivitySources.Inventory.StartActivity("Inventory.ReleaseReservation");
        activity?.SetTag("ecommerce.order.id", notification.OrderId);
        IReadOnlyCollection<StockItem> stockItems = await stockItemRepository.GetByOrderIdAsync(
            notification.OrderId,
            cancellationToken);
        int releasedCount = 0;
        foreach (StockItem stockItem in stockItems)
        {
            releasedCount += stockItem.ReleaseReservationsForOrder(notification.OrderId);
        }

        if (releasedCount == 0)
        {
            InventoryOrderLog.FinalizationIgnored(logger, notification.OrderId);
            return;
        }

        await stockItemRepository.SaveChangesAsync(cancellationToken);
        foreach (StockItem stockItem in stockItems)
        {
            await integrationEventPublisher.PublishAsync(
                new ProductAvailabilityChangedIntegrationEvent(
                    Guid.NewGuid(),
                    stockItem.ProductId,
                    stockItem.AvailableQuantity > 0,
                    DateTimeOffset.UtcNow,
                    notification.CorrelationId),
                cancellationToken);
        }
        InventoryOrderLog.ReservationsReleased(logger, notification.OrderId, releasedCount);
    }
}
#pragma warning restore CA1711
