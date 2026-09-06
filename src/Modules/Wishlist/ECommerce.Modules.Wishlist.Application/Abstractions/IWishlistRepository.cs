using ECommerce.Modules.Wishlist.Domain.WishlistItems;

namespace ECommerce.Modules.Wishlist.Application.Abstractions;

public interface IWishlistRepository
{
    Task<IReadOnlyList<WishlistItem>> GetUserWishlistAsync(Guid userId, CancellationToken cancellationToken);
    Task<WishlistItem?> GetByUserAndProductAsync(Guid userId, Guid productId, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid userId, Guid productId, CancellationToken cancellationToken);
    Task AddAsync(WishlistItem item, CancellationToken cancellationToken);
    Task RemoveAsync(WishlistItem item, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
