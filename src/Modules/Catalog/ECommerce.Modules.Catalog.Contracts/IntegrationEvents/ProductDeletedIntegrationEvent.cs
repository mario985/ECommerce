using ECommerce.Common.Application.Messaging;

namespace ECommerce.Modules.Catalog.Contracts.IntegrationEvents;

public sealed record ProductDeletedIntegrationEvent(
    Guid EventId,
    Guid ProductId,
    DateTimeOffset OccurredAtUtc,
    string CorrelationId = "") : IIntegrationEvent;
