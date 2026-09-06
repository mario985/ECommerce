using ECommerce.Common.Application.Messaging;

namespace ECommerce.Modules.Inventory.Contracts.IntegrationEvents;

public sealed record InventoryReservationConfirmedIntegrationEvent(
    Guid EventId,
    Guid ReservationId,
    Guid ProductId,
    int Quantity,
    DateTimeOffset OccurredAtUtc,
    string CorrelationId = "") : IIntegrationEvent;
