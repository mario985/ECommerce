using ECommerce.Modules.Cart.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using CartAggregate = ECommerce.Modules.Cart.Domain.Carts.Cart;

namespace ECommerce.Modules.Cart.Infrastructure.Persistence.Repositories;

internal sealed class CartRepository(CartDbContext dbContext) : ICartRepository
{
    public Task<CartAggregate?> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        return dbContext.Carts
            .Include(cart => cart.Items)
            .SingleOrDefaultAsync(
                cart => cart.CustomerId == customerId,
                cancellationToken);
    }

    public async Task AddAsync(
        CartAggregate cart,
        CancellationToken cancellationToken)
    {
        await dbContext.Carts.AddAsync(cart, cancellationToken);
    }

    public Task<CartAggregate?> GetByIdAsync(Guid cartId, CancellationToken cancellationToken)
    {
        return dbContext.Carts
            .Include(cart => cart.Items)
            .SingleOrDefaultAsync(cart => cart.Id == cartId, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
