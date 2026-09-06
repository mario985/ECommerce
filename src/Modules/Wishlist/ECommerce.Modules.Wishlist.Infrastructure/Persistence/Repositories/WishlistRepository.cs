using ECommerce.Modules.Wishlist.Application.Abstractions;
using ECommerce.Modules.Wishlist.Domain.WishlistItems;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Wishlist.Infrastructure.Persistence.Repositories;

internal sealed class WishlistRepository(WishlistDbContext dbContext) : IWishlistRepository
{
    public async Task<IReadOnlyList<WishlistItem>> GetUserWishlistAsync(Guid userId, CancellationToken cancellationToken) =>
        (await dbContext.WishlistItems.AsNoTracking().Where(item => item.UserId == userId).ToListAsync(cancellationToken))
        .OrderByDescending(item => item.CreatedAtUtc).ToList();
    public Task<WishlistItem?> GetByUserAndProductAsync(Guid userId, Guid productId, CancellationToken cancellationToken) =>
        dbContext.WishlistItems.SingleOrDefaultAsync(item => item.UserId == userId && item.ProductId == productId, cancellationToken);
    public Task<bool> ExistsAsync(Guid userId, Guid productId, CancellationToken cancellationToken) =>
        dbContext.WishlistItems.AnyAsync(item => item.UserId == userId && item.ProductId == productId, cancellationToken);
    public Task AddAsync(WishlistItem item, CancellationToken cancellationToken) => dbContext.WishlistItems.AddAsync(item, cancellationToken).AsTask();
    public Task RemoveAsync(WishlistItem item, CancellationToken cancellationToken) { dbContext.WishlistItems.Remove(item); return Task.CompletedTask; }
    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
