using ECommerce.Common.Domain.Events;

namespace ECommerce.Modules.Cart.Domain.Carts.Events;

public sealed record CartCheckoutRequestedDomainEvent(
    Guid CheckoutId,
    Guid CartId,
    Guid CustomerId) : IDomainEvent;
