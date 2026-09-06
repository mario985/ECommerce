using CartAggregate = ECommerce.Modules.Cart.Domain.Carts.Cart;

namespace ECommerce.Modules.Cart.Application.Abstractions;

public interface ICartRepository
{
    Task<CartAggregate?> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken);

    Task<CartAggregate?> GetByIdAsync(Guid cartId, CancellationToken cancellationToken);

    Task AddAsync(CartAggregate cart, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
