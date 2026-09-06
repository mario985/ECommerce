using ECommerce.Modules.Identity.Presentation.Authentication;
using ECommerce.Modules.Identity.Presentation.Users;
using ECommerce.Modules.Identity.Presentation.Admin;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.RateLimiting;
using ECommerce.Common.Presentation;

namespace ECommerce.Modules.Identity.Presentation;

public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/v1/identity")
            .RequireRateLimiting(ECommerce.Common.Presentation.RateLimitingPolicyNames.Authentication);
        group.MapAuthenticationEndpoints();
        group.MapUserEndpoints();
        endpoints.MapGroup("/api/v1/admin/identity")
            .RequireRateLimiting(ECommerce.Common.Presentation.RateLimitingPolicyNames.Admin)
            .MapIdentityAdminEndpoints();

        return endpoints;
    }
}
