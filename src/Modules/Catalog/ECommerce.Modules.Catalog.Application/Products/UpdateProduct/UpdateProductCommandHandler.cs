using ECommerce.Common.Application.Messaging;
using ECommerce.Modules.Catalog.Application.Caching;
using ECommerce.Modules.Catalog.Application.Products.CreateProduct;
using ECommerce.Modules.Catalog.Contracts.IntegrationEvents;
using ECommerce.Modules.Catalog.Domain.Products;
using ECommerce.Modules.Catalog.Domain.Categories;
using ECommerce.Modules.Catalog.Application.Products;
using MediatR;

namespace ECommerce.Modules.Catalog.Application.Products.UpdateProduct;

public sealed class UpdateProductCommandHandler(
    IProductRepository productRepository,
    IIntegrationEventPublisher integrationEventPublisher,
    ICatalogCacheInvalidator cacheInvalidator,
    ICategoryRepository? categoryRepository = null)
    : IRequestHandler<UpdateProductCommand>
{
    public async Task Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        Product product = await productRepository.GetByIdAsync(
                request.ProductId,
                cancellationToken)
            ?? throw new ProductNotFoundException(request.ProductId);

        Sku sku = Sku.Create(request.Sku);

        if (await productRepository.ExistsBySkuAsync(
                sku,
                request.ProductId,
                cancellationToken))
        {
            throw new DuplicateProductSkuException(sku);
        }

        Money price = Money.Create(request.Price, request.Currency);
        if (request.CategoryId.HasValue && categoryRepository is not null &&
            await categoryRepository.GetByIdAsync(request.CategoryId.Value, cancellationToken) is null)
        {
            throw new CategoryNotFoundException(request.CategoryId.Value);
        }
        product.Update(
            request.Name,
            request.Description,
            sku,
            price,
            DateTimeOffset.UtcNow,
            request.CategoryId);

        await productRepository.UpdateAsync(product, cancellationToken);
        await cacheInvalidator.InvalidateProductAsync(product.Id, cancellationToken);
        await cacheInvalidator.InvalidateSearchesAsync(cancellationToken);

        await integrationEventPublisher.PublishAsync(
            new ProductUpdatedIntegrationEvent(
                Guid.NewGuid(),
                product.Id,
                product.Sku.Value,
                product.Price.Amount,
                product.Price.Currency,
                DateTimeOffset.UtcNow),
            cancellationToken);
    }
}
