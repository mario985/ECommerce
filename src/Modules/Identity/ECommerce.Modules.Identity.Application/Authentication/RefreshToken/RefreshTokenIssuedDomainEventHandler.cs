using ECommerce.Common.Application.Messaging;
using ECommerce.Modules.Identity.Contracts.IntegrationEvents;
using ECommerce.Modules.Identity.Domain.Users.Events;

namespace ECommerce.Modules.Identity.Application.Authentication.RefreshToken;

#pragma warning disable CA1711 // Domain event handler is the task's explicit terminology.
public sealed class RefreshTokenIssuedDomainEventHandler(
    IIntegrationEventPublisher integrationEventPublisher,
    TimeProvider timeProvider)
{
    public Task HandleAsync(
        RefreshTokenIssuedDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        return integrationEventPublisher.PublishAsync(
            new RefreshTokenIssuedIntegrationEvent(
                Guid.NewGuid(),
                domainEvent.UserId,
                timeProvider.GetUtcNow()),
            cancellationToken);
    }
}
#pragma warning restore CA1711
