using ECommerce.Common.Domain.Events;

namespace ECommerce.Modules.Cart.Domain.Carts.Events;

public sealed record CartClearedDomainEvent(
    Guid CartId,
    Guid CustomerId) : IDomainEvent;
