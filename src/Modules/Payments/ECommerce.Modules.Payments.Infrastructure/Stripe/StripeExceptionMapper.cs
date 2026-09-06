using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Payments.Domain.Payments;
using Stripe;

namespace ECommerce.Modules.Payments.Infrastructure.Stripe;

internal static class StripeExceptionMapper
{
    public static Error Map(StripeException exception)
    {
        if ((int)exception.HttpStatusCode is >= 400 and < 500)
        {
            return new Error(
                PaymentErrors.ProviderRejectedRequestCode,
                PaymentErrors.ProviderRejectedRequestDescription,
                ErrorType.Validation);
        }

        return new Error(
            PaymentErrors.ProviderUnavailableCode,
            PaymentErrors.ProviderUnavailableDescription,
            ErrorType.Failure);
    }

    public static Error Unexpected() => new(
        PaymentErrors.UnexpectedProviderFailureCode,
        PaymentErrors.UnexpectedProviderFailureDescription,
        ErrorType.Failure);
}
