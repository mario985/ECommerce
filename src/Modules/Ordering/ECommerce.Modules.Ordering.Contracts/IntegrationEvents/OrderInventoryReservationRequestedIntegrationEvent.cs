using ECommerce.Common.Application.Messaging;

namespace ECommerce.Modules.Ordering.Contracts.IntegrationEvents;

public sealed record OrderInventoryReservationRequestedIntegrationEvent(
    Guid EventId,
    Guid OrderId,
    Guid CheckoutId,
    Guid CustomerId,
    IReadOnlyCollection<OrderReservationItem> Items,
    DateTimeOffset OccurredAtUtc,
    string CorrelationId = "") : IIntegrationEvent;
