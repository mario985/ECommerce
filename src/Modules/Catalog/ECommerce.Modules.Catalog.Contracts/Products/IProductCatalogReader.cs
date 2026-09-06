namespace ECommerce.Modules.Catalog.Contracts.Products;

public interface IProductCatalogReader
{
    Task<ProductCatalogResponse?> GetByIdAsync(
        Guid productId,
        CancellationToken cancellationToken);
}
