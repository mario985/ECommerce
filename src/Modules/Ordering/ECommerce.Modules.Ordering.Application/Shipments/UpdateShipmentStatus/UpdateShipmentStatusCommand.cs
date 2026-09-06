using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Ordering.Domain.Shipments;
using MediatR;

namespace ECommerce.Modules.Ordering.Application.Shipments.UpdateShipmentStatus;

public sealed record UpdateShipmentStatusCommand(
    Guid ShipmentId,
    ShipmentStatus Status) : IRequest<Result<ShipmentResponse>>;
