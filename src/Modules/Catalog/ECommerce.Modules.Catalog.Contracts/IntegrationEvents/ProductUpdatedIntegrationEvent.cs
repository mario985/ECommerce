using ECommerce.Common.Application.Messaging;

namespace ECommerce.Modules.Catalog.Contracts.IntegrationEvents;

public sealed record ProductUpdatedIntegrationEvent(
    Guid EventId,
    Guid ProductId,
    string Sku,
    decimal Price,
    string Currency,
    DateTimeOffset OccurredAtUtc,
    string CorrelationId = "") : IIntegrationEvent;
