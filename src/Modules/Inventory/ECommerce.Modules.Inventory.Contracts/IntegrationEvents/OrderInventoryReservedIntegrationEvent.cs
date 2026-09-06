using ECommerce.Common.Application.Messaging;

namespace ECommerce.Modules.Inventory.Contracts.IntegrationEvents;

public sealed record OrderInventoryReservedIntegrationEvent(
    Guid EventId,
    Guid OrderId,
    Guid CheckoutId,
    DateTimeOffset OccurredAtUtc,
    string CorrelationId = "") : IIntegrationEvent;
