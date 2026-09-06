namespace ECommerce.Modules.Catalog.Contracts.Products;

public sealed record ProductCatalogResponse(
    Guid Id,
    string Name,
    string Sku,
    decimal Price,
    string Currency,
    bool IsActive);
