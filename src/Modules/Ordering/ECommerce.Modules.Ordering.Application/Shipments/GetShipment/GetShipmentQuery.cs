using ECommerce.Common.Application.Errors;
using MediatR;

namespace ECommerce.Modules.Ordering.Application.Shipments.GetShipment;

public sealed record GetShipmentQuery(Guid ShipmentId) : IRequest<Result<ShipmentResponse>>;
