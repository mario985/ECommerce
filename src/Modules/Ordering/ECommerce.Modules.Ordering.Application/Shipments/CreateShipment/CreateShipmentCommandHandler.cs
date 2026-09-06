using ECommerce.Common.Application.Caching;
using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Ordering.Application.Abstractions;
using ECommerce.Modules.Ordering.Application.Caching;
using ECommerce.Modules.Ordering.Domain.Orders;
using ECommerce.Modules.Ordering.Domain.Shipments;
using MediatR;

namespace ECommerce.Modules.Ordering.Application.Shipments.CreateShipment;

public sealed class CreateShipmentCommandHandler(
    IOrderRepository orderRepository,
    IShipmentRepository shipmentRepository,
    ICacheService cacheService,
    TimeProvider timeProvider)
    : IRequestHandler<CreateShipmentCommand, Result<ShipmentResponse>>
{
    public async Task<Result<ShipmentResponse>> Handle(
        CreateShipmentCommand request,
        CancellationToken cancellationToken)
    {
        Order? order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure<ShipmentResponse>(ShipmentApplicationErrors.OrderNotFound);
        }

        // Reaching Paid requires a successful inventory reservation followed by payment success.
        if (order.Status != OrderStatus.Paid || order.PaymentId is null)
        {
            return Result.Failure<ShipmentResponse>(ShipmentApplicationErrors.OrderNotPaid);
        }

        if (await shipmentRepository.ExistsByOrderIdAsync(request.OrderId, cancellationToken))
        {
            return Result.Failure<ShipmentResponse>(ShipmentApplicationErrors.AlreadyExists);
        }

        Shipment shipment = Shipment.Create(
            request.OrderId,
            request.Carrier,
            request.TrackingNumber,
            timeProvider.GetUtcNow());
        await shipmentRepository.AddAsync(shipment, cancellationToken);
        await shipmentRepository.SaveChangesAsync(cancellationToken);
        await cacheService.RemoveAsync(
            ShipmentTrackingCacheKeys.ForOrder(request.OrderId), cancellationToken);
        return Result.Success(ShipmentResponse.From(shipment));
    }
}
