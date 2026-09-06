using ECommerce.Common.Application.Messaging;
using ECommerce.Modules.Payments.Contracts.IntegrationEvents;
using ECommerce.Modules.Payments.Domain.Payments.Events;

namespace ECommerce.Modules.Payments.Application.EventHandlers;

#pragma warning disable CA1711 // Domain event handler is the architecture's explicit terminology.
public sealed class PaymentFailedDomainEventHandler(
    IIntegrationEventPublisher integrationEventPublisher)
{
    public Task HandleAsync(
        PaymentFailedDomainEvent domainEvent,
        DateTimeOffset occurredAtUtc,
        string correlationId,
        CancellationToken cancellationToken) =>
        integrationEventPublisher.PublishAsync(
            new PaymentFailedIntegrationEvent(
                Guid.NewGuid(),
                domainEvent.PaymentId,
                domainEvent.OrderId,
                domainEvent.CustomerId,
                domainEvent.FailureCode,
                occurredAtUtc,
                correlationId),
            cancellationToken);
}
#pragma warning restore CA1711
