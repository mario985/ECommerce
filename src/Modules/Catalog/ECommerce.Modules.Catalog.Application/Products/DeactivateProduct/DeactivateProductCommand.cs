using MediatR;

namespace ECommerce.Modules.Catalog.Application.Products.DeactivateProduct;

public sealed record DeactivateProductCommand(Guid ProductId) : IRequest;
