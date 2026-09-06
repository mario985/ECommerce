using ECommerce.Common.Domain.Events;

namespace ECommerce.Modules.Ordering.Domain.Orders.Events;

public sealed record OrderPaidDomainEvent(
    Guid OrderId,
    Guid CheckoutId,
    Guid CustomerId,
    Guid PaymentId) : IDomainEvent;
