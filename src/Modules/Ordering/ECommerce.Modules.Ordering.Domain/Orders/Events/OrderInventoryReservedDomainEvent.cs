using ECommerce.Common.Domain.Events;

namespace ECommerce.Modules.Ordering.Domain.Orders.Events;

public sealed record OrderInventoryReservedDomainEvent(
    Guid OrderId,
    Guid CheckoutId,
    Guid CustomerId) : IDomainEvent;
