using MediatR;

namespace ECommerce.Modules.Catalog.Application.Products.DeleteProduct;

public sealed record DeleteProductCommand(Guid ProductId) : IRequest;
