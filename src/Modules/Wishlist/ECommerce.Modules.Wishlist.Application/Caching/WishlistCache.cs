using ECommerce.Common.Application.Caching;

namespace ECommerce.Modules.Wishlist.Application.Caching;

public static class WishlistCache
{
    public static string ForUser(Guid userId) => $"wishlist:{userId}";
    public static Task InvalidateAsync(ICacheService cache, Guid userId, CancellationToken token) =>
        cache.RemoveAsync(ForUser(userId), token);
}
