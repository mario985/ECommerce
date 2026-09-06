using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Payments.Application.Abstractions;
using ECommerce.Modules.Payments.Domain.Payments;

namespace ECommerce.Modules.Payments.Infrastructure.Stripe;

public sealed class StripeAmountConverter : IStripeAmountConverter
{
    private static readonly HashSet<string> ZeroDecimalCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "BIF", "CLP", "DJF", "GNF", "JPY", "KMF", "KRW", "MGA",
        "PYG", "RWF", "UGX", "VND", "VUV", "XAF", "XOF", "XPF",
    };

    private static readonly HashSet<string> ThreeDecimalCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "BHD", "JOD", "KWD", "OMR", "TND",
    };

    private static readonly HashSet<string> TwoDecimalCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "AED", "AUD", "BRL", "CAD", "CHF", "CNY", "CZK", "DKK",
        "EGP", "EUR", "GBP", "HKD", "HUF", "INR", "MXN", "NOK",
        "NZD", "PLN", "RON", "SAR", "SEK", "SGD", "THB", "TRY",
        "USD", "ZAR",
    };

    public Result<long> ToMinorUnits(decimal amount, string currency)
    {
        if (amount <= 0)
        {
            return Failure(
                PaymentErrors.InvalidAmountCode,
                PaymentErrors.InvalidAmountDescription);
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            return UnsupportedCurrency();
        }

        string normalizedCurrency = currency.Trim().ToUpperInvariant();
        int exponent;
        if (ZeroDecimalCurrencies.Contains(normalizedCurrency))
        {
            exponent = 0;
        }
        else if (ThreeDecimalCurrencies.Contains(normalizedCurrency))
        {
            exponent = 3;
        }
        else if (TwoDecimalCurrencies.Contains(normalizedCurrency))
        {
            exponent = 2;
        }
        else
        {
            return UnsupportedCurrency();
        }

        decimal factor = exponent switch
        {
            0 => 1m,
            2 => 100m,
            3 => 1000m,
            _ => throw new InvalidOperationException("Unsupported currency exponent."),
        };
        decimal minorUnits = amount * factor;
        if (minorUnits != decimal.Truncate(minorUnits))
        {
            return Failure(
                PaymentErrors.InvalidPrecisionCode,
                PaymentErrors.InvalidPrecisionDescription);
        }

        if (minorUnits > long.MaxValue)
        {
            return Failure(
                PaymentErrors.InvalidAmountCode,
                PaymentErrors.InvalidAmountDescription);
        }

        return Result.Success((long)minorUnits);
    }

    private static Result<long> UnsupportedCurrency() => Failure(
        PaymentErrors.UnsupportedCurrencyCode,
        PaymentErrors.UnsupportedCurrencyDescription);

    private static Result<long> Failure(string code, string description) =>
        Result.Failure<long>(new Error(code, description, ErrorType.Validation));
}
