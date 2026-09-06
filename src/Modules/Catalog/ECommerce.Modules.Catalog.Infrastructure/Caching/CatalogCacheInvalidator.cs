using ECommerce.Common.Application.Caching;
using ECommerce.Modules.Catalog.Application.Caching;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Catalog.Infrastructure.Caching;

internal sealed partial class CatalogCacheInvalidator(
    ICacheService cacheService,
    CatalogSearchCacheVersion searchCacheVersion,
    ILogger<CatalogCacheInvalidator> logger) : ICatalogCacheInvalidator
{
    public async Task InvalidateProductAsync(Guid productId, CancellationToken cancellationToken)
    {
        string productKey = CatalogCacheKeys.Product(productId);
        string contractKey = CatalogCacheKeys.ProductContract(productId);
        await cacheService.RemoveAsync(productKey, cancellationToken);
        await cacheService.RemoveAsync(contractKey, cancellationToken);
        ProductInvalidated(logger, productId, productKey, contractKey);
    }

    public async Task InvalidateSearchesAsync(CancellationToken cancellationToken)
    {
        string version = await searchCacheVersion.ChangeAsync(cancellationToken);
        SearchesInvalidated(logger, version);
    }

    [LoggerMessage(8201, LogLevel.Information,
        "Catalog Product {ProductId} cache invalidated using keys {ProductCacheKey} and {ContractCacheKey}")]
    private static partial void ProductInvalidated(
        ILogger logger,
        Guid productId,
        string productCacheKey,
        string contractCacheKey);

    [LoggerMessage(8202, LogLevel.Information,
        "Catalog search cache namespace invalidated; current version is {SearchVersion}")]
    private static partial void SearchesInvalidated(ILogger logger, string searchVersion);
}
