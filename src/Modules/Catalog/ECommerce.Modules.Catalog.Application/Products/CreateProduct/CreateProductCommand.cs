using MediatR;

namespace ECommerce.Modules.Catalog.Application.Products.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string? Description,
    string Sku,
    decimal Price,
    string Currency,
    Guid? CategoryId = null) : IRequest<Guid>;
