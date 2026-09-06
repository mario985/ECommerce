using ECommerce.Common.Domain.Events;

namespace ECommerce.Modules.Ordering.Domain.Shipments.Events;

public sealed record ShipmentDeliveredDomainEvent(
    Guid ShipmentId,
    Guid OrderId,
    DateTimeOffset OccurredAtUtc) : IDomainEvent;
