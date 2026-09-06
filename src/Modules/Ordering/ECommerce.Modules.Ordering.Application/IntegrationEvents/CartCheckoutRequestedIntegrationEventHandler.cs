using ECommerce.Modules.Cart.Contracts.IntegrationEvents;
using ECommerce.Common.Application.Observability;
using ECommerce.Modules.Ordering.Application.Abstractions;
using ECommerce.Modules.Ordering.Domain.Orders;
using ECommerce.Modules.Ordering.Domain.Orders.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Ordering.Application.IntegrationEvents;

#pragma warning disable CA1711 // Integration event handler is the architecture's explicit terminology.
public sealed class CartCheckoutRequestedIntegrationEventHandler(
    IOrderRepository orderRepository,
    CartCheckoutRequestedValidator validator,
    OrderCreatedDomainEventHandler domainEventHandler,
    TimeProvider timeProvider,
    ILogger<CartCheckoutRequestedIntegrationEventHandler> logger)
    : INotificationHandler<CartCheckoutRequestedIntegrationEvent>
{
    public async Task Handle(
        CartCheckoutRequestedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        using System.Diagnostics.Activity? activity = CommerceActivitySources.Ordering.StartActivity("Ordering.CreateOrder");
        activity?.SetTag("ecommerce.checkout.id", notification.CheckoutId);
        activity?.SetTag("ecommerce.customer.id", notification.CustomerId);
        OrderLog.CheckoutReceived(logger, notification.CheckoutId, notification.CustomerId);
        if (await orderRepository.ExistsByCheckoutIdAsync(notification.CheckoutId, cancellationToken))
        {
            OrderLog.DuplicateCheckoutIgnored(logger, notification.CheckoutId, notification.CustomerId);
            return;
        }

        if (!(await validator.ValidateAsync(notification, cancellationToken)).IsValid)
        {
            OrderLog.InvalidCheckoutRejected(logger, notification.CheckoutId, notification.CustomerId);
            return;
        }

        OrderItemSnapshot[] items = notification.Items
            .Select(item => new OrderItemSnapshot(
                item.ProductId,
                item.ProductName,
                item.Sku,
                item.UnitPrice,
                item.Currency,
                item.Quantity))
            .ToArray();
        Order order = Order.Create(
            notification.CheckoutId,
            notification.CustomerId,
            items,
            timeProvider.GetUtcNow());
        await orderRepository.AddAsync(order, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);
        OrderLog.Created(logger, order.Id, order.CheckoutId, order.CustomerId);

        OrderCreatedDomainEvent domainEvent = order.DomainEvents
            .OfType<OrderCreatedDomainEvent>()
            .Single();
        order.ClearDomainEvents();
        activity?.SetTag("ecommerce.order.id", order.Id);
        await domainEventHandler.HandleAsync(
            domainEvent,
            order,
            notification.CorrelationId,
            cancellationToken);
    }
}
#pragma warning restore CA1711
