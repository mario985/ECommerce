using ECommerce.Modules.Payments.Domain.Payments;

namespace ECommerce.Modules.Payments.Application.Payments.GetPayment;

public sealed record PaymentResponse(
    Guid Id,
    Guid OrderId,
    decimal Amount,
    string Currency,
    string Status,
    string Provider,
    string? ProviderPaymentIntentId,
    string? ClientSecret,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc,
    IReadOnlyCollection<PaymentAttemptResponse> Attempts)
{
    internal static PaymentResponse From(Payment payment) => new(
        payment.Id,
        payment.OrderId,
        payment.Amount,
        payment.Currency,
        payment.Status.ToString(),
        payment.Provider.ToString(),
        payment.ProviderPaymentIntentId,
        payment.Status is PaymentStatus.Succeeded or PaymentStatus.Failed or PaymentStatus.Cancelled
            ? null
            : payment.ClientSecret,
        payment.CreatedAtUtc,
        payment.UpdatedAtUtc,
        payment.Attempts.Select(PaymentAttemptResponse.From).ToArray());
}
