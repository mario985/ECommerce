using ECommerce.Common.Domain.Events;

namespace ECommerce.Modules.Ordering.Domain.Shipments.Events;

public sealed record ShipmentCreatedDomainEvent(
    Guid ShipmentId,
    Guid OrderId,
    DateTimeOffset OccurredAtUtc) : IDomainEvent;
