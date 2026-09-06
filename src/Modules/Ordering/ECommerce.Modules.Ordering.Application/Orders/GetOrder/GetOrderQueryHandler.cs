using ECommerce.Common.Application.Authentication;
using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Ordering.Application.Abstractions;
using ECommerce.Modules.Ordering.Domain.Orders;
using MediatR;

namespace ECommerce.Modules.Ordering.Application.Orders.GetOrder;

public sealed class GetOrderQueryHandler(
    IOrderRepository orderRepository,
    ICurrentUser currentUser)
    : IRequestHandler<GetOrderQuery, Result<OrderResponse>>
{
    public async Task<Result<OrderResponse>> Handle(
        GetOrderQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            return Failure(
                OrderErrors.UnauthorizedCode,
                OrderErrors.UnauthorizedDescription,
                ErrorType.Unauthorized);
        }

        Order? order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Failure(OrderErrors.NotFoundCode, OrderErrors.NotFoundDescription, ErrorType.NotFound);
        }

        if (order.CustomerId != currentUser.UserId.Value)
        {
            return Failure(OrderErrors.ForbiddenCode, OrderErrors.ForbiddenDescription, ErrorType.Forbidden);
        }

        return Result.Success(OrderResponse.From(order));
    }

    private static Result<OrderResponse> Failure(string code, string description, ErrorType type) =>
        Result.Failure<OrderResponse>(new Error(code, description, type));
}
