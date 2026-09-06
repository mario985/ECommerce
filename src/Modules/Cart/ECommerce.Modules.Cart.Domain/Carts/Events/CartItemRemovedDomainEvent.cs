using ECommerce.Common.Domain.Events;

namespace ECommerce.Modules.Cart.Domain.Carts.Events;

public sealed record CartItemRemovedDomainEvent(
    Guid CartId,
    Guid CustomerId,
    Guid ProductId) : IDomainEvent;
