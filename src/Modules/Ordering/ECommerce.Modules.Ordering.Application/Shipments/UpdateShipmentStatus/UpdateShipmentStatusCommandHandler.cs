using ECommerce.Common.Application.Caching;
using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Ordering.Application.Abstractions;
using ECommerce.Modules.Ordering.Application.Caching;
using ECommerce.Modules.Ordering.Domain.Orders;
using ECommerce.Modules.Ordering.Domain.Shipments;
using MediatR;

namespace ECommerce.Modules.Ordering.Application.Shipments.UpdateShipmentStatus;

public sealed class UpdateShipmentStatusCommandHandler(
    IShipmentRepository shipmentRepository,
    IOrderRepository orderRepository,
    ICacheService cacheService,
    TimeProvider timeProvider)
    : IRequestHandler<UpdateShipmentStatusCommand, Result<ShipmentResponse>>
{
    public async Task<Result<ShipmentResponse>> Handle(
        UpdateShipmentStatusCommand request,
        CancellationToken cancellationToken)
    {
        Shipment? shipment = await shipmentRepository.GetByIdAsync(
            request.ShipmentId, cancellationToken);
        if (shipment is null)
        {
            return Result.Failure<ShipmentResponse>(ShipmentApplicationErrors.NotFound);
        }

        if (request.Status == ShipmentStatus.Preparing)
        {
            Order? order = await orderRepository.GetByIdAsync(shipment.OrderId, cancellationToken);
            if (order is null)
            {
                return Result.Failure<ShipmentResponse>(ShipmentApplicationErrors.OrderNotFound);
            }

            if (order.Status != OrderStatus.Paid || order.PaymentId is null)
            {
                return Result.Failure<ShipmentResponse>(ShipmentApplicationErrors.OrderNotPaid);
            }
        }

        DateTimeOffset now = timeProvider.GetUtcNow();
        ShipmentTransitionOutcome outcome = request.Status switch
        {
            ShipmentStatus.Pending when shipment.Status == ShipmentStatus.Pending =>
                ShipmentTransitionOutcome.AlreadyApplied,
            ShipmentStatus.Preparing => shipment.StartPreparing(now),
            ShipmentStatus.Shipped => shipment.MarkAsShipped(now),
            ShipmentStatus.OutForDelivery => shipment.MarkAsOutForDelivery(now),
            ShipmentStatus.Delivered => shipment.MarkAsDelivered(now),
            ShipmentStatus.Cancelled => shipment.Cancel(now),
            _ => ShipmentTransitionOutcome.InvalidState,
        };

        if (outcome == ShipmentTransitionOutcome.AlreadyDelivered)
        {
            return Result.Failure<ShipmentResponse>(ShipmentApplicationErrors.AlreadyDelivered);
        }

        if (outcome == ShipmentTransitionOutcome.InvalidState)
        {
            return Result.Failure<ShipmentResponse>(ShipmentApplicationErrors.InvalidStatusTransition);
        }

        if (outcome == ShipmentTransitionOutcome.Applied)
        {
            await shipmentRepository.SaveChangesAsync(cancellationToken);
            await cacheService.RemoveAsync(
                ShipmentTrackingCacheKeys.ForOrder(shipment.OrderId), cancellationToken);
        }

        return Result.Success(ShipmentResponse.From(shipment));
    }
}
