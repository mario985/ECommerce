using ECommerce.Common.Domain.Events;

namespace ECommerce.Modules.Cart.Domain.Carts.Events;

public sealed record CartItemPurchasedQuantityRemovedDomainEvent(
    Guid CartId,
    Guid CustomerId,
    Guid CheckoutId,
    Guid ProductId,
    int Quantity) : IDomainEvent;
