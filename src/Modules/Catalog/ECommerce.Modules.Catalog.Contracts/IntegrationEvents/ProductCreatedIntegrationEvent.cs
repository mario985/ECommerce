using ECommerce.Common.Application.Messaging;

namespace ECommerce.Modules.Catalog.Contracts.IntegrationEvents;

public sealed record ProductCreatedIntegrationEvent(
    Guid EventId,
    Guid ProductId,
    string Sku,
    DateTimeOffset OccurredAtUtc,
    string CorrelationId = "") : IIntegrationEvent;
