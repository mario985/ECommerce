using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Ordering.Domain.Orders;
using MediatR;

namespace ECommerce.Modules.Ordering.Application.Orders.GetOrderHistory;

public sealed record GetOrderHistoryQuery(
    int Page,
    int PageSize,
    OrderStatus? Status) : IRequest<Result<OrderHistoryResponse>>;
