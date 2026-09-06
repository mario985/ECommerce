using ECommerce.Modules.Catalog.Domain.Products;

namespace ECommerce.Modules.Catalog.Infrastructure.MongoDb;

internal static class ProductDocumentMapper
{
    public static ProductDocument ToDocument(Product product)
    {
        return new ProductDocument
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            CategoryId = product.CategoryId,
            Sku = product.Sku.Value,
            PriceAmount = product.Price.Amount,
            PriceCurrency = product.Price.Currency,
            IsActive = product.IsActive,
            CreatedAtUtc = product.CreatedAtUtc,
            UpdatedAtUtc = product.UpdatedAtUtc,
            CreatedBy = product.CreatedBy,
            UpdatedBy = product.UpdatedBy,
        };
    }

    public static Product ToProduct(ProductDocument document)
    {
        return Product.Rehydrate(
            document.Id,
            document.Name,
            document.Description,
            Sku.Create(document.Sku),
            Money.Create(document.PriceAmount, document.PriceCurrency),
            document.IsActive,
            document.CreatedAtUtc,
            document.UpdatedAtUtc,
            document.CreatedBy,
            document.UpdatedBy,
            document.CategoryId);
    }
}
