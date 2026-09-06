using ECommerce.Common.Application.Messaging;
using ECommerce.Modules.Catalog.Application.Caching;
using ECommerce.Modules.Catalog.Application.Products.UpdateProduct;
using ECommerce.Modules.Catalog.Contracts.IntegrationEvents;
using ECommerce.Modules.Catalog.Domain.Products;
using MediatR;

namespace ECommerce.Modules.Catalog.Application.Products.DeleteProduct;

public sealed class DeleteProductCommandHandler(
    IProductRepository productRepository,
    IIntegrationEventPublisher integrationEventPublisher,
    ICatalogCacheInvalidator cacheInvalidator)
    : IRequestHandler<DeleteProductCommand>
{
    public async Task Handle(
        DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        Product product = await productRepository.GetByIdAsync(
                request.ProductId,
                cancellationToken)
            ?? throw new ProductNotFoundException(request.ProductId);

        await productRepository.DeleteAsync(product.Id, cancellationToken);
        await cacheInvalidator.InvalidateProductAsync(product.Id, cancellationToken);
        await cacheInvalidator.InvalidateSearchesAsync(cancellationToken);

        await integrationEventPublisher.PublishAsync(
            new ProductDeletedIntegrationEvent(
                Guid.NewGuid(),
                product.Id,
                DateTimeOffset.UtcNow),
            cancellationToken);
    }
}
