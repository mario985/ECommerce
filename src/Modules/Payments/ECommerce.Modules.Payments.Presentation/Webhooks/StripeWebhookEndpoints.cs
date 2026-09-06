using System.Text;
using ECommerce.Common.Application.Errors;
using ECommerce.Common.Presentation.Results;
using ECommerce.Modules.Payments.Application.Webhooks.ProcessStripeWebhook;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.Modules.Payments.Presentation.Webhooks;

public static class StripeWebhookEndpoints
{
    public static RouteGroupBuilder MapStripeWebhookRoutes(this RouteGroupBuilder group)
    {
        group.MapPost("/webhooks/stripe", ProcessAsync)
            .WithName("Payments.StripeWebhook")
            .AllowAnonymous()
            .DisableRateLimiting();
        return group;
    }

    private static async Task<IResult> ProcessAsync(
        HttpRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        using StreamReader reader = new(
            request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);
        string payload = await reader.ReadToEndAsync(cancellationToken);
        string signature = request.Headers["Stripe-Signature"].ToString();
        Result<StripeWebhookResult> result = await sender.Send(
            new ProcessStripeWebhookCommand(payload, signature),
            cancellationToken);
        return result.IsFailure
            ? ApiResults.Problem(result.Error!)
            : Results.Ok(result.Value);
    }
}
