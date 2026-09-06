using ECommerce.Common.Application.Caching;
using ECommerce.Modules.Reviews.Application.Caching;

namespace ECommerce.Modules.Reviews.Infrastructure.Caching;

internal sealed class ReviewCacheInvalidator(ICacheService cacheService) : IReviewCacheInvalidator
{
    public Task InvalidateProductAsync(Guid productId, CancellationToken cancellationToken) =>
        cacheService.SetAsync(
            ReviewCacheKeys.ProductVersion(productId),
            Guid.NewGuid().ToString("N"),
            new CacheEntryOptions(),
            cancellationToken);
}
