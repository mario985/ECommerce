using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Ordering.Application.Abstractions;
using ECommerce.Modules.Ordering.Domain.Shipments;
using MediatR;

namespace ECommerce.Modules.Ordering.Application.Shipments.GetShipment;

public sealed class GetShipmentQueryHandler(IShipmentRepository shipmentRepository)
    : IRequestHandler<GetShipmentQuery, Result<ShipmentResponse>>
{
    public async Task<Result<ShipmentResponse>> Handle(
        GetShipmentQuery request,
        CancellationToken cancellationToken)
    {
        Shipment? shipment = await shipmentRepository.GetByIdAsync(
            request.ShipmentId, cancellationToken);
        return shipment is null
            ? Result.Failure<ShipmentResponse>(ShipmentApplicationErrors.NotFound)
            : Result.Success(ShipmentResponse.From(shipment));
    }
}
