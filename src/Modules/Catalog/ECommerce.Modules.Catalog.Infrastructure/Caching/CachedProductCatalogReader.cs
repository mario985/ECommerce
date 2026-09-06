using ECommerce.Common.Application.Caching;
using ECommerce.Modules.Catalog.Application.Caching;
using ECommerce.Modules.Catalog.Contracts.Products;

namespace ECommerce.Modules.Catalog.Infrastructure.Caching;

internal sealed class CachedProductCatalogReader(
    MongoDb.MongoProductCatalogReader innerReader,
    ICacheService cacheService,
    CatalogCacheOptions cacheOptions) : IProductCatalogReader
{
    public async Task<ProductCatalogResponse?> GetByIdAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        string cacheKey = CatalogCacheKeys.ProductContract(productId);
        ProductCatalogResponse? cached = await cacheService.GetAsync<ProductCatalogResponse>(
            cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        ProductCatalogResponse? response = await innerReader.GetByIdAsync(
            productId, cancellationToken);
        if (response is null)
        {
            return null;
        }

        await cacheService.SetAsync(
            cacheKey,
            response,
            new CacheEntryOptions(cacheOptions.ProductExpiration),
            cancellationToken);
        return response;
    }
}
