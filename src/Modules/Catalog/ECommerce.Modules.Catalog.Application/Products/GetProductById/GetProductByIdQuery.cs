using MediatR;

namespace ECommerce.Modules.Catalog.Application.Products.GetProductById;

public sealed record GetProductByIdQuery(Guid ProductId) : IRequest<ProductResponse?>;
