using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Payments.Application.Abstractions;
using ECommerce.Modules.Payments.Domain.Payments;
using Microsoft.Extensions.Options;
using Stripe;

namespace ECommerce.Modules.Payments.Infrastructure.Stripe;

internal sealed class StripeWebhookVerifier(IOptions<StripeOptions> options)
    : IPaymentWebhookVerifier
{
    public Result<PaymentWebhookEvent> Verify(string payload, string signature)
    {
        if (string.IsNullOrWhiteSpace(payload) || string.IsNullOrWhiteSpace(signature))
        {
            return Invalid();
        }

        try
        {
            global::Stripe.Event stripeEvent = EventUtility.ConstructEvent(
                payload,
                signature,
                options.Value.WebhookSecret,
                tolerance: 300,
                throwOnApiVersionMismatch: false);
            PaymentIntent? paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (string.IsNullOrWhiteSpace(stripeEvent.Id) ||
                string.IsNullOrWhiteSpace(stripeEvent.Type) ||
                IsSupportedPaymentIntentEvent(stripeEvent.Type) &&
                string.IsNullOrWhiteSpace(paymentIntent?.Id))
            {
                return Invalid();
            }

            return Result.Success(new PaymentWebhookEvent(
                stripeEvent.Id,
                stripeEvent.Type,
                paymentIntent?.Id ?? string.Empty,
                paymentIntent?.Status ?? string.Empty,
                paymentIntent?.LastPaymentError?.Code,
                SafeFailureMessage(paymentIntent?.LastPaymentError?.Message),
                stripeEvent.Created));
        }
        catch (Exception)
        {
            return Invalid();
        }
    }

    private static bool IsSupportedPaymentIntentEvent(string eventType) => eventType is
        "payment_intent.processing" or
        "payment_intent.succeeded" or
        "payment_intent.payment_failed";

    private static string? SafeFailureMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        string trimmed = message.Trim();
        return trimmed.Length <= 500 ? trimmed : trimmed[..500];
    }

    private static Result<PaymentWebhookEvent> Invalid() =>
        Result.Failure<PaymentWebhookEvent>(new Error(
            PaymentErrors.InvalidWebhookCode,
            PaymentErrors.InvalidWebhookDescription,
            ErrorType.Validation));
}
