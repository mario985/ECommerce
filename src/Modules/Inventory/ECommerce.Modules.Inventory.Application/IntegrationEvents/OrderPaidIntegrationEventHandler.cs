using ECommerce.Modules.Inventory.Application.Reservations.ReserveOrderInventory;
using ECommerce.Common.Application.Observability;
using ECommerce.Modules.Inventory.Domain.StockItems;
using ECommerce.Modules.Ordering.Contracts.IntegrationEvents;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Inventory.Application.IntegrationEvents;

#pragma warning disable CA1711 // Integration event handler is the architecture's explicit terminology.
public sealed class OrderPaidIntegrationEventHandler(
    IStockItemRepository stockItemRepository,
    ILogger<OrderPaidIntegrationEventHandler> logger)
    : INotificationHandler<OrderPaidIntegrationEvent>
{
    public async Task Handle(
        OrderPaidIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        using System.Diagnostics.Activity? activity = CommerceActivitySources.Inventory.StartActivity("Inventory.ConfirmReservation");
        activity?.SetTag("ecommerce.order.id", notification.OrderId);
        IReadOnlyCollection<StockItem> stockItems = await stockItemRepository.GetByOrderIdAsync(
            notification.OrderId,
            cancellationToken);
        int confirmedCount = 0;
        foreach (StockItem stockItem in stockItems)
        {
            confirmedCount += stockItem.ConfirmReservationsForOrder(notification.OrderId);
        }

        if (confirmedCount == 0)
        {
            InventoryOrderLog.FinalizationIgnored(logger, notification.OrderId);
            return;
        }

        await stockItemRepository.SaveChangesAsync(cancellationToken);
        InventoryOrderLog.ReservationsConfirmed(
            logger, notification.OrderId, confirmedCount);
    }
}
#pragma warning restore CA1711
