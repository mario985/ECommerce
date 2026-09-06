using ECommerce.Common.Application.Messaging;
using ECommerce.Common.Application.Observability;
using ECommerce.Modules.Inventory.Contracts.IntegrationEvents;
using ECommerce.Modules.Inventory.Domain.StockItems;
using ECommerce.Modules.Ordering.Contracts.IntegrationEvents;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Inventory.Application.Reservations.ReserveOrderInventory;

#pragma warning disable CA1711 // Integration event handler is the architecture's explicit terminology.
public sealed class OrderInventoryReservationRequestedIntegrationEventHandler(
    IStockItemRepository stockItemRepository,
    IIntegrationEventPublisher integrationEventPublisher,
    OrderInventoryReservationRequestedValidator validator,
    TimeProvider timeProvider,
    ILogger<OrderInventoryReservationRequestedIntegrationEventHandler> logger)
    : INotificationHandler<OrderInventoryReservationRequestedIntegrationEvent>
{
    public async Task Handle(
        OrderInventoryReservationRequestedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        using System.Diagnostics.Activity? activity = CommerceActivitySources.Inventory.StartActivity("Inventory.Reserve");
        activity?.SetTag("ecommerce.order.id", notification.OrderId);
        activity?.SetTag("ecommerce.checkout.id", notification.CheckoutId);
        InventoryOrderLog.ReservationReceived(logger, notification.OrderId, notification.CheckoutId);
        if (!(await validator.ValidateAsync(notification, cancellationToken)).IsValid)
        {
            await PublishFailureAsync(
                notification,
                productId: null,
                "The reservation request is invalid.",
                cancellationToken);
            return;
        }

        List<(StockItem StockItem, OrderReservationItem Item)> reservations =
            new(notification.Items.Count);
        foreach (OrderReservationItem item in notification.Items)
        {
            StockItem? stockItem = await stockItemRepository.GetByProductIdAsync(
                item.ProductId,
                cancellationToken);
            if (stockItem is null)
            {
                await PublishFailureAsync(
                    notification,
                    item.ProductId,
                    "The stock item could not be found.",
                    cancellationToken);
                return;
            }

            if (stockItem.AvailableQuantity < item.Quantity)
            {
                await PublishFailureAsync(
                    notification,
                    item.ProductId,
                    "Insufficient available stock.",
                    cancellationToken);
                return;
            }

            reservations.Add((stockItem, item));
        }

        DateTimeOffset occurredAtUtc = timeProvider.GetUtcNow();
        foreach ((StockItem stockItem, OrderReservationItem item) in reservations)
        {
            stockItem.Reserve(item.Quantity, occurredAtUtc, notification.OrderId);
        }

        await stockItemRepository.SaveChangesAsync(cancellationToken);
        foreach ((StockItem stockItem, _) in reservations)
        {
            await integrationEventPublisher.PublishAsync(
                new ProductAvailabilityChangedIntegrationEvent(
                    Guid.NewGuid(),
                    stockItem.ProductId,
                    stockItem.AvailableQuantity > 0,
                    occurredAtUtc,
                    notification.CorrelationId),
                cancellationToken);
        }
        await integrationEventPublisher.PublishAsync(
            new OrderInventoryReservedIntegrationEvent(
                Guid.NewGuid(),
                notification.OrderId,
                notification.CheckoutId,
                timeProvider.GetUtcNow(),
                notification.CorrelationId),
            cancellationToken);
        InventoryOrderLog.ReservationSucceeded(
            logger,
            notification.OrderId,
            notification.CheckoutId,
            reservations.Count);
    }

    private async Task PublishFailureAsync(
        OrderInventoryReservationRequestedIntegrationEvent notification,
        Guid? productId,
        string reason,
        CancellationToken cancellationToken)
    {
        await integrationEventPublisher.PublishAsync(
            new OrderInventoryReservationFailedIntegrationEvent(
                Guid.NewGuid(),
                notification.OrderId,
                notification.CheckoutId,
                productId,
                reason,
                timeProvider.GetUtcNow(),
                notification.CorrelationId),
            cancellationToken);
        InventoryOrderLog.ReservationFailed(
            logger,
            notification.OrderId,
            notification.CheckoutId,
            productId,
            reason);
    }
}
#pragma warning restore CA1711
