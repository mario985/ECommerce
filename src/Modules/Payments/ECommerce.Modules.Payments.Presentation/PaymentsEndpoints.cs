using ECommerce.Modules.Payments.Presentation.Payments;
using ECommerce.Modules.Payments.Presentation.Webhooks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using ECommerce.Common.Presentation;

namespace ECommerce.Modules.Payments.Presentation;

public static class PaymentsEndpoints
{
    public static IEndpointRouteBuilder MapPaymentsEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGroup("/api/v1/payments")
            .RequireAuthorization()
            .RequireRateLimiting(ECommerce.Common.Presentation.RateLimitingPolicyNames.Authenticated)
            .MapPaymentRoutes();
        endpoints.MapGroup("/api/v1/payments")
            .MapStripeWebhookRoutes();
        return endpoints;
    }
}
