using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Ordering.Domain.Shipments;
using MediatR;

namespace ECommerce.Modules.Ordering.Application.Shipments.GetShipments;

public sealed record GetShipmentsQuery(
    int Page,
    int PageSize,
    ShipmentStatus? Status) : IRequest<Result<ShipmentsResponse>>;
