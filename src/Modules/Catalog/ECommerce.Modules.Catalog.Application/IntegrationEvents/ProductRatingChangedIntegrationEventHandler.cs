using ECommerce.Modules.Catalog.Application.Caching;
using ECommerce.Modules.Catalog.Domain.Products;
using ECommerce.Modules.Reviews.Contracts.IntegrationEvents;
using MediatR;

namespace ECommerce.Modules.Catalog.Application.IntegrationEvents;

#pragma warning disable CA1711
public sealed class ProductRatingChangedIntegrationEventHandler(
    IProductRepository productRepository,
    ICatalogCacheInvalidator cacheInvalidator)
    : INotificationHandler<ProductRatingChangedIntegrationEvent>
{
    public async Task Handle(
        ProductRatingChangedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        await productRepository.UpdateRatingAsync(
            notification.ProductId,
            notification.AverageRating,
            notification.ReviewCount,
            cancellationToken);
        await cacheInvalidator.InvalidateSearchesAsync(cancellationToken);
    }
}
#pragma warning restore CA1711
