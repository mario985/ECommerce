using ECommerce.Common.Domain.Events;

namespace ECommerce.Modules.Cart.Domain.Checkouts.Events;

public sealed record CartCheckoutCreatedDomainEvent(
    Guid CheckoutId, Guid CartId, Guid CustomerId) : IDomainEvent;
