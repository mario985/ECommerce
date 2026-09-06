using ECommerce.Modules.Catalog.Contracts.Products;
using MongoDB.Driver;

namespace ECommerce.Modules.Catalog.Infrastructure.MongoDb;

internal sealed class MongoProductCatalogReader(
    IMongoCollection<ProductDocument> productsCollection) : IProductCatalogReader
{
    public async Task<ProductCatalogResponse?> GetByIdAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        ProductDocument? product = await productsCollection
            .Find(document => document.Id == productId)
            .FirstOrDefaultAsync(cancellationToken);

        return product is null
            ? null
            : new ProductCatalogResponse(
                product.Id,
                product.Name,
                product.Sku,
                product.PriceAmount,
                product.PriceCurrency,
                product.IsActive);
    }
}
