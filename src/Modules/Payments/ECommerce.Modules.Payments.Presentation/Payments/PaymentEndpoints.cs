using ECommerce.Common.Application.Errors;
using ECommerce.Common.Presentation.Results;
using ECommerce.Modules.Payments.Application.Payments.GetPayment;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Modules.Payments.Presentation.Payments;

public static class PaymentEndpoints
{
    public static RouteGroupBuilder MapPaymentRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/orders/{orderId:guid}", GetPaymentAsync)
            .WithName("Payments.GetPayment")
            .WithTags("Payments");
        return group;
    }

    private static async Task<IResult> GetPaymentAsync(
        Guid orderId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        Result<PaymentResponse> result = await sender.Send(
            new GetPaymentQuery(orderId),
            cancellationToken);
        return result.IsFailure ? ApiResults.Problem(result.Error!) : Results.Ok(result.Value);
    }
}
