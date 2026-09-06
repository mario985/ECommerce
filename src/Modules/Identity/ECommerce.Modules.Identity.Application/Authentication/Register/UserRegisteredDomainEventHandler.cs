using ECommerce.Common.Application.Messaging;
using ECommerce.Modules.Identity.Contracts.IntegrationEvents;
using ECommerce.Modules.Identity.Domain.Users.Events;

namespace ECommerce.Modules.Identity.Application.Authentication.Register;

#pragma warning disable CA1711 // The task explicitly requires domain event handler terminology.
public sealed class UserRegisteredDomainEventHandler(
    IIntegrationEventPublisher integrationEventPublisher,
    TimeProvider timeProvider)
{
    public Task HandleAsync(
        UserRegisteredDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        return integrationEventPublisher.PublishAsync(
            new UserRegisteredIntegrationEvent(
                Guid.NewGuid(),
                domainEvent.UserId,
                domainEvent.Email,
                timeProvider.GetUtcNow()),
            cancellationToken);
    }
}
#pragma warning restore CA1711
