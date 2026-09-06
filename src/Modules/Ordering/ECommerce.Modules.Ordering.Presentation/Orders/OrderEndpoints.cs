using ECommerce.Common.Application.Errors;
using ECommerce.Common.Presentation.Results;
using ECommerce.Modules.Ordering.Application.Orders.GetOrder;
using ECommerce.Modules.Ordering.Application.Orders.GetOrderHistory;
using ECommerce.Modules.Ordering.Domain.Orders;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Modules.Ordering.Presentation.Orders;

public static class OrderEndpoints
{
    public static RouteGroupBuilder MapOrderRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/{orderId:guid}", GetOrderAsync).WithName("Ordering.GetOrder").WithTags("Ordering");
        group.MapGet("", GetHistoryAsync).WithName("Ordering.GetHistory").WithTags("Ordering");
        return group;
    }

    private static async Task<IResult> GetOrderAsync(
        Guid orderId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        Result<OrderResponse> result = await sender.Send(
            new GetOrderQuery(orderId),
            cancellationToken);
        return result.IsFailure ? ApiResults.Problem(result.Error!) : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetHistoryAsync(
        ISender sender,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20,
        OrderStatus? status = null)
    {
        Result<OrderHistoryResponse> result = await sender.Send(
            new GetOrderHistoryQuery(page, pageSize, status),
            cancellationToken);
        return result.IsFailure ? ApiResults.Problem(result.Error!) : Results.Ok(result.Value);
    }
}
