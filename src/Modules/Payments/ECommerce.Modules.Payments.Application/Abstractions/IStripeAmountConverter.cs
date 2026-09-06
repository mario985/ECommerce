using ECommerce.Common.Application.Errors;

namespace ECommerce.Modules.Payments.Application.Abstractions;

public interface IStripeAmountConverter
{
    Result<long> ToMinorUnits(decimal amount, string currency);
}
