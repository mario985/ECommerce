using ECommerce.Common.Domain.Entities;

namespace ECommerce.Modules.Payments.Domain.Payments;

public sealed class PaymentAttempt : Entity<Guid>
{
    private PaymentAttempt()
    {
    }

    internal PaymentAttempt(
        Guid id,
        Guid paymentId,
        string? providerReference,
        PaymentStatus status,
        DateTimeOffset createdAtUtc,
        string? failureCode,
        string? failureMessage)
        : base(id)
    {
        PaymentId = paymentId;
        ProviderReference = providerReference;
        Status = status;
        CreatedAtUtc = createdAtUtc;
        FailureCode = NormalizeOptional(failureCode, 100);
        FailureMessage = NormalizeOptional(failureMessage, 500);
    }

    public Guid PaymentId { get; private set; }
    public string? ProviderReference { get; private set; }
    public PaymentStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public string? FailureCode { get; private set; }
    public string? FailureMessage { get; private set; }

    internal void MarkProcessing()
    {
        Status = PaymentStatus.Processing;
        FailureCode = null;
        FailureMessage = null;
    }

    internal void MarkSucceeded()
    {
        Status = PaymentStatus.Succeeded;
        FailureCode = null;
        FailureMessage = null;
    }

    internal void MarkFailed(string? failureCode, string? failureMessage)
    {
        Status = PaymentStatus.Failed;
        FailureCode = NormalizeOptional(failureCode, 100);
        FailureMessage = NormalizeOptional(failureMessage, 500);
    }

    private static string? NormalizeOptional(string? value, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        return trimmed.Length <= maximumLength ? trimmed : trimmed[..maximumLength];
    }
}
