using ECommerce.Common.Application.Messaging;
using ECommerce.Modules.Catalog.Application.Caching;
using ECommerce.Modules.Catalog.Application.Products.UpdateProduct;
using ECommerce.Modules.Catalog.Contracts.IntegrationEvents;
using ECommerce.Modules.Catalog.Domain.Products;
using MediatR;

namespace ECommerce.Modules.Catalog.Application.Products.DeactivateProduct;

public sealed class DeactivateProductCommandHandler(
    IProductRepository productRepository,
    IIntegrationEventPublisher integrationEventPublisher,
    ICatalogCacheInvalidator cacheInvalidator)
    : IRequestHandler<DeactivateProductCommand>
{
    public async Task Handle(
        DeactivateProductCommand request,
        CancellationToken cancellationToken)
    {
        Product product = await productRepository.GetByIdAsync(
                request.ProductId,
                cancellationToken)
            ?? throw new ProductNotFoundException(request.ProductId);

        DateTimeOffset occurredAtUtc = DateTimeOffset.UtcNow;
        product.Deactivate(occurredAtUtc);

        await productRepository.UpdateAsync(product, cancellationToken);
        await cacheInvalidator.InvalidateProductAsync(product.Id, cancellationToken);
        await cacheInvalidator.InvalidateSearchesAsync(cancellationToken);

        await integrationEventPublisher.PublishAsync(
            new ProductDeactivatedIntegrationEvent(
                Guid.NewGuid(),
                product.Id,
                occurredAtUtc),
            cancellationToken);
    }
}
