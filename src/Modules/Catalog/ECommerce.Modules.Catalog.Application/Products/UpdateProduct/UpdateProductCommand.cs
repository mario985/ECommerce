using MediatR;

namespace ECommerce.Modules.Catalog.Application.Products.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid ProductId,
    string Name,
    string? Description,
    string Sku,
    decimal Price,
    string Currency,
    Guid? CategoryId = null) : IRequest;
