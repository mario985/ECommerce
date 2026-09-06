using MediatR;

namespace ECommerce.Common.Application.Messaging;

public interface IIntegrationEvent : INotification
{
    Guid EventId { get; }

    string CorrelationId { get; }

    DateTimeOffset OccurredAtUtc { get; }
}
