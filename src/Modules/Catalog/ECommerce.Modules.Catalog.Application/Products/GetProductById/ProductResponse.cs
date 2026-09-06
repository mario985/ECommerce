using ECommerce.Modules.Catalog.Domain.Products;

namespace ECommerce.Modules.Catalog.Application.Products.GetProductById;

public sealed record ProductResponse(
    Guid Id,
    string Name,
    string? Description,
    string Sku,
    decimal Price,
    string Currency,
    bool IsActive,
    Guid? CategoryId = null)
{
    public static ProductResponse FromProduct(Product product)
    {
        return new ProductResponse(
            product.Id,
            product.Name,
            product.Description,
            product.Sku.Value,
            product.Price.Amount,
            product.Price.Currency,
            product.IsActive,
            product.CategoryId);
    }
}
