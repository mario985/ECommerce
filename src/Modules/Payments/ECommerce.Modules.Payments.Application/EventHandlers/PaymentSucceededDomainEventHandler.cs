using ECommerce.Common.Application.Messaging;
using ECommerce.Modules.Payments.Contracts.IntegrationEvents;
using ECommerce.Modules.Payments.Domain.Payments;
using ECommerce.Modules.Payments.Domain.Payments.Events;

namespace ECommerce.Modules.Payments.Application.EventHandlers;

#pragma warning disable CA1711 // Domain event handler is the architecture's explicit terminology.
public sealed class PaymentSucceededDomainEventHandler(
    IIntegrationEventPublisher integrationEventPublisher)
{
    public Task HandleAsync(
        PaymentSucceededDomainEvent domainEvent,
        Payment payment,
        DateTimeOffset occurredAtUtc,
        CancellationToken cancellationToken) =>
        integrationEventPublisher.PublishAsync(
            new PaymentSucceededIntegrationEvent(
                Guid.NewGuid(),
                domainEvent.PaymentId,
                domainEvent.OrderId,
                domainEvent.CustomerId,
                payment.Amount,
                payment.Currency,
                occurredAtUtc,
                payment.CorrelationId),
            cancellationToken);
}
#pragma warning restore CA1711
