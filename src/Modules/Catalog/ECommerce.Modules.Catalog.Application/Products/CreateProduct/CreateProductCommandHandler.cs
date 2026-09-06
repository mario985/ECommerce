using ECommerce.Common.Application.Messaging;
using ECommerce.Modules.Catalog.Application.Caching;
using ECommerce.Modules.Catalog.Contracts.IntegrationEvents;
using ECommerce.Modules.Catalog.Domain.Products;
using ECommerce.Modules.Catalog.Domain.Categories;
using ECommerce.Modules.Catalog.Application.Products;
using MediatR;

namespace ECommerce.Modules.Catalog.Application.Products.CreateProduct;

public sealed class CreateProductCommandHandler(
    IProductRepository productRepository,
    IIntegrationEventPublisher integrationEventPublisher,
    ICatalogCacheInvalidator cacheInvalidator,
    ICategoryRepository? categoryRepository = null)
    : IRequestHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        Sku sku = Sku.Create(request.Sku);

        if (await productRepository.ExistsBySkuAsync(
                sku,
                excludingProductId: null,
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
        Product product = Product.Create(request.Name, request.Description, sku, price, request.CategoryId);

        await productRepository.AddAsync(product, cancellationToken);
        await cacheInvalidator.InvalidateSearchesAsync(cancellationToken);

        await integrationEventPublisher.PublishAsync(
            new ProductCreatedIntegrationEvent(
                Guid.NewGuid(),
                product.Id,
                product.Sku.Value,
                DateTimeOffset.UtcNow),
            cancellationToken);

        return product.Id;
    }
}
