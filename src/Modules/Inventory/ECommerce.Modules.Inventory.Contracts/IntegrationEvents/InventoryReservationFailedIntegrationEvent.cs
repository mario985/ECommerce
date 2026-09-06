using ECommerce.Common.Application.Messaging;

namespace ECommerce.Modules.Inventory.Contracts.IntegrationEvents;

public sealed record InventoryReservationFailedIntegrationEvent(
    Guid EventId,
    Guid ProductId,
    int Quantity,
    string Reason,
    DateTimeOffset OccurredAtUtc,
    string CorrelationId = "") : IIntegrationEvent;
