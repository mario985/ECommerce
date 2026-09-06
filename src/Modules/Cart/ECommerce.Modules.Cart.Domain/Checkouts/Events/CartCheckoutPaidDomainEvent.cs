using ECommerce.Common.Domain.Events;

namespace ECommerce.Modules.Cart.Domain.Checkouts.Events;

public sealed record CartCheckoutPaidDomainEvent(
    Guid CheckoutId, Guid CartId, Guid CustomerId, Guid OrderId) : IDomainEvent;
