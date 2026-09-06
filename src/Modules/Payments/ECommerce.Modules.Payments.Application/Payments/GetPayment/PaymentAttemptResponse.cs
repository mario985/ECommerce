using ECommerce.Modules.Payments.Domain.Payments;

namespace ECommerce.Modules.Payments.Application.Payments.GetPayment;

public sealed record PaymentAttemptResponse(
    Guid Id,
    string? ProviderReference,
    string Status,
    DateTimeOffset CreatedAtUtc,
    string? FailureCode,
    string? FailureMessage)
{
    internal static PaymentAttemptResponse From(PaymentAttempt attempt) => new(
        attempt.Id,
        attempt.ProviderReference,
        attempt.Status.ToString(),
        attempt.CreatedAtUtc,
        attempt.FailureCode,
        attempt.FailureMessage);
}
