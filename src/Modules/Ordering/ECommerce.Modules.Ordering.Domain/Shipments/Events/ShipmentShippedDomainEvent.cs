using ECommerce.Common.Domain.Events;

namespace ECommerce.Modules.Ordering.Domain.Shipments.Events;

public sealed record ShipmentShippedDomainEvent(
    Guid ShipmentId,
    Guid OrderId,
    DateTimeOffset OccurredAtUtc) : IDomainEvent;
