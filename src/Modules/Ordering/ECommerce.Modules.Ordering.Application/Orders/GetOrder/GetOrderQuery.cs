using ECommerce.Common.Application.Errors;
using MediatR;

namespace ECommerce.Modules.Ordering.Application.Orders.GetOrder;

public sealed record GetOrderQuery(Guid OrderId) : IRequest<Result<OrderResponse>>;
