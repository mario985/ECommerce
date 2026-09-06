using ECommerce.Modules.Cart.Presentation.Carts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using ECommerce.Common.Presentation;

namespace ECommerce.Modules.Cart.Presentation;

public static class CartModuleEndpoints
{
    public static IEndpointRouteBuilder MapCartEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGroup("/api/v1/cart")
            .RequireAuthorization()
            .RequireRateLimiting(ECommerce.Common.Presentation.RateLimitingPolicyNames.Authenticated)
            .MapCartRoutes();

        return endpoints;
    }
}
