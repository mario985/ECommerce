using ECommerce.Common.Application.Messaging;
using ECommerce.Modules.Ordering.Contracts.IntegrationEvents;
using ECommerce.Modules.Ordering.Domain.Orders;
using ECommerce.Modules.Ordering.Domain.Orders.Events;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Ordering.Application.IntegrationEvents;

#pragma warning disable CA1711 // Domain event handler is the architecture's explicit terminology.
public sealed class OrderCreatedDomainEventHandler(
    IIntegrationEventPublisher integrationEventPublisher,
    TimeProvider timeProvider,
    ILogger<OrderCreatedDomainEventHandler> logger)
{
    public async Task HandleAsync(
        OrderCreatedDomainEvent domainEvent,
        Order order,
        string correlationId,
        CancellationToken cancellationToken)
    {
        OrderInventoryReservationRequestedIntegrationEvent integrationEvent = new(
            Guid.NewGuid(),
            domainEvent.OrderId,
            domainEvent.CheckoutId,
            domainEvent.CustomerId,
            order.Lines
                .Select(line => new OrderReservationItem(line.ProductId, line.Quantity))
                .ToArray(),
            timeProvider.GetUtcNow(),
            correlationId);

        await integrationEventPublisher.PublishAsync(integrationEvent, cancellationToken);
        OrderLog.InventoryRequested(
            logger,
            domainEvent.OrderId,
            domainEvent.CheckoutId,
            domainEvent.CustomerId);
    }
}
#pragma warning restore CA1711
