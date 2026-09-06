using ECommerce.Common.Application.Errors;
using MediatR;

namespace ECommerce.Modules.Ordering.Application.Shipments.GetOrderTracking;

public sealed record GetOrderTrackingQuery(Guid OrderId) : IRequest<Result<OrderTrackingResponse>>;
