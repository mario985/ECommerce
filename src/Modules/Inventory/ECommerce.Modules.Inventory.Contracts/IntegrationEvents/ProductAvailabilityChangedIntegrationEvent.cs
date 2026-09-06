using ECommerce.Common.Application.Messaging;

namespace ECommerce.Modules.Inventory.Contracts.IntegrationEvents;

public sealed record ProductAvailabilityChangedIntegrationEvent(
    Guid EventId,
    Guid ProductId,
    bool InStock,
    DateTimeOffset OccurredAtUtc,
    string CorrelationId = "") : IIntegrationEvent;
