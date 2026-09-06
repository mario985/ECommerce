using ECommerce.Modules.Ordering.Presentation.Orders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using ECommerce.Common.Presentation;
using ECommerce.Modules.Ordering.Presentation.Shipments;

namespace ECommerce.Modules.Ordering.Presentation;

public static class OrderingEndpoints
{
    public static IEndpointRouteBuilder MapOrderingEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGroup("/api/v1/orders")
            .RequireAuthorization()
            .RequireRateLimiting(ECommerce.Common.Presentation.RateLimitingPolicyNames.Authenticated)
            .MapOrderRoutes()
            .MapShipmentTrackingRoute();
        endpoints.MapShipmentEndpoints();
        return endpoints;
    }
}
