using ECommerce.Modules.Catalog.Application.Caching;
using ECommerce.Modules.Catalog.Domain.Products;
using ECommerce.Modules.Inventory.Contracts.IntegrationEvents;
using MediatR;

namespace ECommerce.Modules.Catalog.Application.IntegrationEvents;

#pragma warning disable CA1711
public sealed class ProductAvailabilityChangedIntegrationEventHandler(
    IProductRepository productRepository,
    ICatalogCacheInvalidator cacheInvalidator)
    : INotificationHandler<ProductAvailabilityChangedIntegrationEvent>
{
    public async Task Handle(
        ProductAvailabilityChangedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        await productRepository.UpdateAvailabilityAsync(
            notification.ProductId,
            notification.InStock,
            cancellationToken);
        await cacheInvalidator.InvalidateSearchesAsync(cancellationToken);
    }
}
#pragma warning restore CA1711
