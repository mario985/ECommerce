using ECommerce.Common.Domain.Events;

namespace ECommerce.Modules.Ordering.Domain.Orders.Events;

public sealed record OrderInventoryReservationFailedDomainEvent(
    Guid OrderId,
    Guid CheckoutId,
    Guid CustomerId) : IDomainEvent;
