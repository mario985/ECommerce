using ECommerce.Modules.Inventory.Contracts.IntegrationEvents;
using ECommerce.Modules.Ordering.Application.Abstractions;
using ECommerce.Modules.Ordering.Domain.Orders;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Ordering.Application.IntegrationEvents;

#pragma warning disable CA1711 // Integration event handler is the architecture's explicit terminology.
public sealed class OrderInventoryReservationFailedIntegrationEventHandler(
    IOrderRepository orderRepository,
    TimeProvider timeProvider,
    ILogger<OrderInventoryReservationFailedIntegrationEventHandler> logger)
    : INotificationHandler<OrderInventoryReservationFailedIntegrationEvent>
{
    public async Task Handle(
        OrderInventoryReservationFailedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        Order? order = await orderRepository.GetByIdAsync(notification.OrderId, cancellationToken);
        if (order is null)
        {
            OrderLog.OutcomeOrderMissing(logger, notification.OrderId, notification.CheckoutId);
            return;
        }

        OrderTransitionOutcome outcome = order.MarkInventoryReservationFailed(timeProvider.GetUtcNow());
        if (outcome != OrderTransitionOutcome.Applied)
        {
            OrderLog.OutcomeIgnored(logger, order.Id, order.CheckoutId, order.Status.ToString());
            return;
        }

        await orderRepository.SaveChangesAsync(cancellationToken);
        order.ClearDomainEvents();
        OrderLog.InventoryFailed(
            logger,
            order.Id,
            order.CheckoutId,
            order.CustomerId,
            notification.ProductId,
            notification.Reason);
    }
}
#pragma warning restore CA1711
