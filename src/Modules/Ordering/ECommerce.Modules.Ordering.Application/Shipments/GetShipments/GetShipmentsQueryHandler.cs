using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Ordering.Application.Abstractions;
using ECommerce.Modules.Ordering.Domain.Shipments;
using MediatR;

namespace ECommerce.Modules.Ordering.Application.Shipments.GetShipments;

public sealed class GetShipmentsQueryHandler(IShipmentRepository shipmentRepository)
    : IRequestHandler<GetShipmentsQuery, Result<ShipmentsResponse>>
{
    public async Task<Result<ShipmentsResponse>> Handle(
        GetShipmentsQuery request,
        CancellationToken cancellationToken)
    {
        (IReadOnlyCollection<Shipment> shipments, int totalCount) =
            await shipmentRepository.GetShipmentsAsync(
                request.Status, request.Page, request.PageSize, cancellationToken);
        return Result.Success(new ShipmentsResponse(
            shipments.Select(ShipmentResponse.From).ToArray(),
            request.Page,
            request.PageSize,
            totalCount,
            totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)request.PageSize)));
    }
}
