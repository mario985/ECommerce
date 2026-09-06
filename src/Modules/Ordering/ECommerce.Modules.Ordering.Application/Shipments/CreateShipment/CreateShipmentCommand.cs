using ECommerce.Common.Application.Errors;
using MediatR;

namespace ECommerce.Modules.Ordering.Application.Shipments.CreateShipment;

public sealed record CreateShipmentCommand(
    Guid OrderId,
    string Carrier,
    string TrackingNumber) : IRequest<Result<ShipmentResponse>>;
