using ECommerce.Common.Application.Messaging;

namespace ECommerce.Modules.Catalog.Contracts.IntegrationEvents;

public sealed record ProductActivatedIntegrationEvent(
    Guid EventId,
    Guid ProductId,
    DateTimeOffset OccurredAtUtc,
    string CorrelationId = "") : IIntegrationEvent;
