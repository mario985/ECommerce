using ECommerce.Modules.Cart.Domain.Checkouts;

namespace ECommerce.Modules.Cart.Application.Abstractions;

public interface ICartCheckoutRepository
{
    Task<CartCheckout?> GetByCheckoutIdAsync(Guid checkoutId, CancellationToken cancellationToken);
    Task<CartCheckout?> GetPendingByCartIdAsync(Guid cartId, CancellationToken cancellationToken);
    Task AddAsync(CartCheckout checkout, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
