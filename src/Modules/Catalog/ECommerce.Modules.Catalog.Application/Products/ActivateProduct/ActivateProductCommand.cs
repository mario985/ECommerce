using MediatR;

namespace ECommerce.Modules.Catalog.Application.Products.ActivateProduct;

public sealed record ActivateProductCommand(Guid ProductId) : IRequest;
