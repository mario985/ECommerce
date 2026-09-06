using ECommerce.Common.Domain.Events;

namespace ECommerce.Modules.Ordering.Domain.Orders.Events;

public sealed record OrderCreatedDomainEvent(
    Guid OrderId,
    Guid CheckoutId,
    Guid CustomerId) : IDomainEvent;
