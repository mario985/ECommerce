using ECommerce.Common.Application.Messaging;
using ECommerce.Modules.Identity.Contracts.IntegrationEvents;
using ECommerce.Modules.Identity.Domain.Users.Events;

namespace ECommerce.Modules.Identity.Application.Authentication.Login;

#pragma warning disable CA1711 // The task explicitly requires domain event handler terminology.
public sealed class UserLoggedInDomainEventHandler(
    IIntegrationEventPublisher integrationEventPublisher,
    TimeProvider timeProvider)
{
    public Task HandleAsync(
        UserLoggedInDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        return integrationEventPublisher.PublishAsync(
            new UserLoggedInIntegrationEvent(
                Guid.NewGuid(),
                domainEvent.UserId,
                timeProvider.GetUtcNow()),
            cancellationToken);
    }
}
#pragma warning restore CA1711
