namespace ECommerce.Common.Application.Messaging;

public interface IIntegrationEventPublisher
{
    Task PublishAsync<TEvent>(
        TEvent integrationEvent,
        CancellationToken cancellationToken)
        where TEvent : IIntegrationEvent;
}
