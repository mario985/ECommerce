using ECommerce.Common.Application.Messaging;
using ECommerce.Common.Application.Observability;
using ECommerce.Modules.Ordering.Application.Abstractions;
using ECommerce.Modules.Ordering.Contracts.IntegrationEvents;
using ECommerce.Modules.Ordering.Domain.Orders;
using ECommerce.Modules.Ordering.Domain.Orders.Events;
using ECommerce.Modules.Payments.Contracts.IntegrationEvents;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Ordering.Application.IntegrationEvents;

#pragma warning disable CA1711 // Integration event handler is the architecture's explicit terminology.
public sealed class PaymentSucceededIntegrationEventHandler(
    IOrderRepository orderRepository,
    IIntegrationEventPublisher integrationEventPublisher,
    ILogger<PaymentSucceededIntegrationEventHandler> logger)
    : INotificationHandler<PaymentSucceededIntegrationEvent>
{
    public async Task Handle(
        PaymentSucceededIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        using System.Diagnostics.Activity? activity = CommerceActivitySources.Ordering.StartActivity("Ordering.MarkPaid");
        activity?.SetTag("ecommerce.order.id", notification.OrderId);
        activity?.SetTag("ecommerce.payment.id", notification.PaymentId);
        Order? order = await orderRepository.GetByIdAsync(notification.OrderId, cancellationToken);
        if (order is null || order.CustomerId != notification.CustomerId)
        {
            OrderLog.PaymentOutcomeOrderMissing(logger, notification.OrderId, notification.PaymentId);
            return;
        }

        OrderTransitionOutcome outcome = order.MarkPaid(
            notification.PaymentId,
            notification.OccurredAtUtc);
        if (outcome != OrderTransitionOutcome.Applied)
        {
            OrderLog.PaymentOutcomeIgnored(
                logger, order.Id, order.CheckoutId, order.Status.ToString());
            return;
        }

        await orderRepository.SaveChangesAsync(cancellationToken);
        OrderPaidDomainEvent domainEvent = order.DomainEvents
            .OfType<OrderPaidDomainEvent>()
            .Single();
        order.ClearDomainEvents();
        await integrationEventPublisher.PublishAsync(
            new OrderPaidIntegrationEvent(
                Guid.NewGuid(),
                domainEvent.OrderId,
                domainEvent.CheckoutId,
                domainEvent.CustomerId,
                notification.OccurredAtUtc,
                notification.CorrelationId),
            cancellationToken);
        OrderLog.MarkedPaid(logger, order.Id, order.CheckoutId, notification.PaymentId);
        activity?.SetTag("ecommerce.order.status", order.Status.ToString());
    }
}
#pragma warning restore CA1711
