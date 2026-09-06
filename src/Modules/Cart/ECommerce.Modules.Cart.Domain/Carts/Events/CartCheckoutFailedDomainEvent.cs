using ECommerce.Common.Domain.Events;

namespace ECommerce.Modules.Cart.Domain.Carts.Events;

public sealed record CartCheckoutFailedDomainEvent(
    Guid CartId,
    Guid CustomerId,
    Guid CheckoutId) : IDomainEvent;
