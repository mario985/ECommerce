using ECommerce.Common.Application.Messaging;
using ECommerce.Modules.Identity.Contracts.IntegrationEvents;
using ECommerce.Modules.Identity.Domain.Users.Events;

namespace ECommerce.Modules.Identity.Application.Authentication.RefreshToken;

#pragma warning disable CA1711 // Domain event handler is the task's explicit terminology.
public sealed class RefreshTokenRevokedDomainEventHandler(
    IIntegrationEventPublisher integrationEventPublisher,
    TimeProvider timeProvider)
{
    public Task HandleAsync(
        RefreshTokenRevokedDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        return integrationEventPublisher.PublishAsync(
            new RefreshTokenRevokedIntegrationEvent(
                Guid.NewGuid(),
                domainEvent.UserId,
                timeProvider.GetUtcNow()),
            cancellationToken);
    }
}
#pragma warning restore CA1711
