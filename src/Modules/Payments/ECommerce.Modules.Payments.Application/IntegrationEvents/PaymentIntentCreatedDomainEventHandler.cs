using ECommerce.Common.Application.Messaging;
using ECommerce.Modules.Payments.Contracts.IntegrationEvents;
using ECommerce.Modules.Payments.Domain.Payments;
using ECommerce.Modules.Payments.Domain.Payments.Events;

namespace ECommerce.Modules.Payments.Application.IntegrationEvents;

#pragma warning disable CA1711 // Domain event handler is the architecture's explicit terminology.
public sealed class PaymentIntentCreatedDomainEventHandler(
    IIntegrationEventPublisher integrationEventPublisher,
    TimeProvider timeProvider)
{
    public Task HandleAsync(
        PaymentIntentCreatedDomainEvent domainEvent,
        Payment payment,
        CancellationToken cancellationToken)
    {
        return integrationEventPublisher.PublishAsync(
            new PaymentIntentCreatedIntegrationEvent(
                Guid.NewGuid(),
                domainEvent.PaymentId,
                domainEvent.OrderId,
                domainEvent.CustomerId,
                payment.Amount,
                payment.Currency,
                timeProvider.GetUtcNow(),
                payment.CorrelationId),
            cancellationToken);
    }
}
#pragma warning restore CA1711
