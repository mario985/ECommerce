using ECommerce.Common.Application.Authentication;
using ECommerce.Common.Application.Caching;
using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Ordering.Application.Abstractions;
using ECommerce.Modules.Ordering.Application.Caching;
using ECommerce.Modules.Ordering.Domain.Orders;
using ECommerce.Modules.Ordering.Domain.Shipments;
using MediatR;

namespace ECommerce.Modules.Ordering.Application.Shipments.GetOrderTracking;

public sealed class GetOrderTrackingQueryHandler(
    IOrderRepository orderRepository,
    IShipmentRepository shipmentRepository,
    ICurrentUser currentUser,
    ICacheService cacheService,
    ShipmentTrackingCacheOptions cacheOptions)
    : IRequestHandler<GetOrderTrackingQuery, Result<OrderTrackingResponse>>
{
    public async Task<Result<OrderTrackingResponse>> Handle(
        GetOrderTrackingQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            return Result.Failure<OrderTrackingResponse>(ShipmentApplicationErrors.Unauthorized);
        }

        Order? order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure<OrderTrackingResponse>(ShipmentApplicationErrors.OrderNotFound);
        }

        if (order.CustomerId != currentUser.UserId.Value)
        {
            return Result.Failure<OrderTrackingResponse>(ShipmentApplicationErrors.Forbidden);
        }

        string cacheKey = ShipmentTrackingCacheKeys.ForOrder(request.OrderId);
        OrderTrackingResponse? cached = await cacheService.GetAsync<OrderTrackingResponse>(
            cacheKey, cancellationToken);
        if (cached is not null)
        {
            return Result.Success(cached);
        }

        Shipment? shipment = await shipmentRepository.GetByOrderIdAsync(
            request.OrderId, cancellationToken);
        if (shipment is null)
        {
            return Result.Failure<OrderTrackingResponse>(ShipmentApplicationErrors.NotFound);
        }

        OrderTrackingResponse response = OrderTrackingResponse.From(shipment);
        await cacheService.SetAsync(
            cacheKey,
            response,
            new CacheEntryOptions(cacheOptions.Expiration),
            cancellationToken);
        return Result.Success(response);
    }
}
