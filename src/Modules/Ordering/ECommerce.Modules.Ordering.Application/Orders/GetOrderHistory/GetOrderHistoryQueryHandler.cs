using ECommerce.Common.Application.Authentication;
using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Ordering.Application.Abstractions;
using ECommerce.Modules.Ordering.Domain.Orders;
using MediatR;

namespace ECommerce.Modules.Ordering.Application.Orders.GetOrderHistory;

public sealed class GetOrderHistoryQueryHandler(
    IOrderRepository orderRepository,
    ICurrentUser currentUser,
    GetOrderHistoryValidator validator)
    : IRequestHandler<GetOrderHistoryQuery, Result<OrderHistoryResponse>>
{
    public async Task<Result<OrderHistoryResponse>> Handle(
        GetOrderHistoryQuery request,
        CancellationToken cancellationToken)
    {
        if (!(await validator.ValidateAsync(request, cancellationToken)).IsValid)
        {
            return Failure(OrderErrors.InvalidHistoryCode, OrderErrors.InvalidHistoryDescription, ErrorType.Validation);
        }

        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            return Failure(OrderErrors.UnauthorizedCode, OrderErrors.UnauthorizedDescription, ErrorType.Unauthorized);
        }

        (IReadOnlyCollection<Order> orders, int totalCount) =
            await orderRepository.GetHistoryAsync(
                currentUser.UserId.Value,
                request.Status,
                request.Page,
                request.PageSize,
                cancellationToken);
        int totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return Result.Success(new OrderHistoryResponse(
            orders.Select(OrderSummaryResponse.From).ToArray(),
            request.Page,
            request.PageSize,
            totalCount,
            totalPages));
    }

    private static Result<OrderHistoryResponse> Failure(
        string code,
        string description,
        ErrorType type) =>
        Result.Failure<OrderHistoryResponse>(new Error(code, description, type));
}
