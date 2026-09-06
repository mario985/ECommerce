using ECommerce.Common.Domain.Events;

namespace ECommerce.Modules.Ordering.Domain.Orders.Events;

public sealed record OrderPaymentFailedDomainEvent(
    Guid OrderId,
    Guid CheckoutId,
    Guid CustomerId) : IDomainEvent;
