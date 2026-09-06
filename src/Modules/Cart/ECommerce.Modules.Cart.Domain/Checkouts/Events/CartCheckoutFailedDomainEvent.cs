using ECommerce.Common.Domain.Events;

namespace ECommerce.Modules.Cart.Domain.Checkouts.Events;

public sealed record CartCheckoutFailedDomainEvent(
    Guid CheckoutId, Guid CartId, Guid CustomerId, Guid OrderId) : IDomainEvent;
