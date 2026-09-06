using ECommerce.Common.Application.Messaging;
using ECommerce.Common.Domain.Events;
using ECommerce.Modules.Identity.Contracts.IntegrationEvents;
using ECommerce.Modules.Identity.Domain.Users.Events;

namespace ECommerce.Modules.Identity.Application.Admin;

#pragma warning disable CA1711 // The task explicitly requires domain event handler terminology.
public sealed class UserAdministrationDomainEventHandler(IIntegrationEventPublisher integrationEventPublisher)
{
    public Task HandleAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        return domainEvent switch
        {
            UserRoleAssignedDomainEvent e => integrationEventPublisher.PublishAsync(
                new UserRoleAssignedIntegrationEvent(Guid.NewGuid(), e.UserId, e.Role, e.OccurredAtUtc),
                cancellationToken),
            UserRoleRemovedDomainEvent e => integrationEventPublisher.PublishAsync(
                new UserRoleRemovedIntegrationEvent(Guid.NewGuid(), e.UserId, e.Role, e.OccurredAtUtc),
                cancellationToken),
            UserDisabledDomainEvent e => integrationEventPublisher.PublishAsync(
                new UserDisabledIntegrationEvent(Guid.NewGuid(), e.UserId, e.OccurredAtUtc),
                cancellationToken),
            UserEnabledDomainEvent e => integrationEventPublisher.PublishAsync(
                new UserEnabledIntegrationEvent(Guid.NewGuid(), e.UserId, e.OccurredAtUtc),
                cancellationToken),
            _ => throw new InvalidOperationException($"Unsupported Identity administration event '{domainEvent.GetType().Name}'."),
        };
    }
}
#pragma warning restore CA1711
