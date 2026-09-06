namespace ECommerce.Modules.Catalog.Presentation.Products;

public sealed record CreateProductRequest(
    string Name,
    string? Description,
    string Sku,
    decimal Price,
    string Currency,
    Guid? CategoryId = null);
