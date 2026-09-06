using ECommerce.Common.Application.Messaging;

namespace ECommerce.Modules.Inventory.Contracts.IntegrationEvents;

public sealed record OrderInventoryReservationFailedIntegrationEvent(
    Guid EventId,
    Guid OrderId,
    Guid CheckoutId,
    Guid? ProductId,
    string Reason,
    DateTimeOffset OccurredAtUtc,
    string CorrelationId = "") : IIntegrationEvent;
