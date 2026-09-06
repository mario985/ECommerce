using ECommerce.Modules.Cart.Application.Abstractions;
using ECommerce.Modules.Cart.Domain.Checkouts;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Cart.Infrastructure.Persistence.Repositories;

internal sealed class CartCheckoutRepository(CartDbContext dbContext) : ICartCheckoutRepository
{
    public Task<CartCheckout?> GetByCheckoutIdAsync(Guid checkoutId, CancellationToken cancellationToken) =>
        dbContext.CartCheckouts.Include(checkout => checkout.Items)
            .SingleOrDefaultAsync(checkout => checkout.CheckoutId == checkoutId, cancellationToken);

    public Task<CartCheckout?> GetPendingByCartIdAsync(Guid cartId, CancellationToken cancellationToken) =>
        dbContext.CartCheckouts.Include(checkout => checkout.Items)
            .SingleOrDefaultAsync(
                checkout => checkout.CartId == cartId && checkout.Status == CartCheckoutStatus.Pending,
                cancellationToken);

    public async Task AddAsync(CartCheckout checkout, CancellationToken cancellationToken) =>
        await dbContext.CartCheckouts.AddAsync(checkout, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
