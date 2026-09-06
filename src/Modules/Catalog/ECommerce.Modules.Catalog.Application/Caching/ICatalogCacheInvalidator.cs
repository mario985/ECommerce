namespace ECommerce.Modules.Catalog.Application.Caching;

public interface ICatalogCacheInvalidator
{
    Task InvalidateProductAsync(Guid productId, CancellationToken cancellationToken);
    Task InvalidateSearchesAsync(CancellationToken cancellationToken);
}
