namespace ECommerce.Modules.Catalog.Presentation.Products;

public sealed record UpdateProductRequest(
    string Name,
    string? Description,
    string Sku,
    decimal Price,
    string Currency,
    Guid? CategoryId = null);
