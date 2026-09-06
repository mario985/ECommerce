using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Payments.Infrastructure.Stripe;

internal static partial class StripeLog
{
    [LoggerMessage(3101, LogLevel.Warning,
        "Stripe request {StripeRequestId} failed with HTTP {StatusCode} and provider code {ProviderCode}")]
    public static partial void RequestFailed(
        ILogger logger,
        string? stripeRequestId,
        int? statusCode,
        string? providerCode);

    [LoggerMessage(3102, LogLevel.Error,
        "Unexpected Stripe transport failure while creating a PaymentIntent")]
    public static partial void UnexpectedFailure(ILogger logger, Exception exception);
}
