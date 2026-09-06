using ECommerce.Common.Application.Messaging;
using ECommerce.Common.Application.Observability;
using ECommerce.Modules.Cart.Contracts.IntegrationEvents;
using ECommerce.Modules.Cart.Domain.Carts.Events;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Cart.Application.Carts.Checkout;

#pragma warning disable CA1711 // Domain event handler is the architecture's explicit terminology.
public sealed class CartCheckoutRequestedDomainEventHandler(
    IIntegrationEventPublisher integrationEventPublisher,
    TimeProvider timeProvider,
    ILogger<CartCheckoutRequestedDomainEventHandler> logger,
    ICorrelationContext? correlationContext = null)
{
    public async Task HandleAsync(
        CartCheckoutRequestedDomainEvent domainEvent,
        IReadOnlyCollection<CartCheckoutItem> items,
        CancellationToken cancellationToken)
    {
        CartCheckoutRequestedIntegrationEvent integrationEvent = new(
            Guid.NewGuid(),
            domainEvent.CheckoutId,
            domainEvent.CartId,
            domainEvent.CustomerId,
            items,
            timeProvider.GetUtcNow(),
            correlationContext?.CorrelationId ?? Guid.NewGuid().ToString("N"));

        await integrationEventPublisher.PublishAsync(integrationEvent, cancellationToken);
        CartLog.CheckoutEventPublished(
            logger,
            integrationEvent.EventId,
            domainEvent.CheckoutId,
            domainEvent.CartId,
            domainEvent.CustomerId);
    }
}
#pragma warning restore CA1711
