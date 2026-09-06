using ECommerce.Common.Domain.Events;

namespace ECommerce.Modules.Cart.Domain.Carts.Events;

public sealed record CartItemQuantityChangedDomainEvent(
    Guid CartId,
    Guid CustomerId,
    Guid ProductId,
    int Quantity) : IDomainEvent;
