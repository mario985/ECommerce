using ECommerce.Common.Application.Messaging;
using ECommerce.Modules.Inventory.Contracts.IntegrationEvents;
using ECommerce.Modules.Ordering.Application.Abstractions;
using ECommerce.Modules.Ordering.Contracts.IntegrationEvents;
using ECommerce.Modules.Ordering.Domain.Orders;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Ordering.Application.IntegrationEvents;

#pragma warning disable CA1711 // Integration event handler is the architecture's explicit terminology.
public sealed class OrderInventoryReservedIntegrationEventHandler(
    IOrderRepository orderRepository,
    IIntegrationEventPublisher integrationEventPublisher,
    TimeProvider timeProvider,
    ILogger<OrderInventoryReservedIntegrationEventHandler> logger)
    : INotificationHandler<OrderInventoryReservedIntegrationEvent>
{
    public async Task Handle(
        OrderInventoryReservedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        Order? order = await orderRepository.GetByIdAsync(notification.OrderId, cancellationToken);
        if (order is null)
        {
            OrderLog.OutcomeOrderMissing(logger, notification.OrderId, notification.CheckoutId);
            return;
        }

        OrderTransitionOutcome outcome = order.MarkInventoryReserved(timeProvider.GetUtcNow());
        if (outcome != OrderTransitionOutcome.Applied)
        {
            OrderLog.OutcomeIgnored(logger, order.Id, order.CheckoutId, order.Status.ToString());
            return;
        }

        await orderRepository.SaveChangesAsync(cancellationToken);
        order.ClearDomainEvents();
        await integrationEventPublisher.PublishAsync(
            new OrderAwaitingPaymentIntegrationEvent(
                Guid.NewGuid(),
                order.Id,
                order.CustomerId,
                order.TotalAmount,
                order.Currency,
                timeProvider.GetUtcNow(),
                notification.CorrelationId),
            cancellationToken);
        OrderLog.AwaitingPayment(logger, order.Id, order.CheckoutId, order.CustomerId);
    }
}
#pragma warning restore CA1711
