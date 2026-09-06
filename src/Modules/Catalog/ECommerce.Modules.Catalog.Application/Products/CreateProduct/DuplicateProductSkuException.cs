using ECommerce.Modules.Catalog.Domain.Products;

namespace ECommerce.Modules.Catalog.Application.Products.CreateProduct;

public sealed class DuplicateProductSkuException : Exception
{
    public DuplicateProductSkuException(Sku sku)
        : base($"A product with SKU '{sku.Value}' already exists.")
    {
        Sku = sku;
    }

    public Sku Sku { get; }
}
