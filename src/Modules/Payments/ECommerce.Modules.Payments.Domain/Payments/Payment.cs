using ECommerce.Common.Domain.Entities;
using ECommerce.Modules.Payments.Domain.Payments.Events;

namespace ECommerce.Modules.Payments.Domain.Payments;

public sealed class Payment : AuditableAggregateRoot<Guid>
{
    private readonly List<PaymentAttempt> _attempts = [];

    private Payment()
    {
        Currency = string.Empty;
        CorrelationId = string.Empty;
    }

    private Payment(
        Guid id,
        Guid orderId,
        Guid customerId,
        decimal amount,
        string currency,
        DateTimeOffset createdAtUtc,
        string correlationId)
        : base(id)
    {
        OrderId = orderId;
        CustomerId = customerId;
        Amount = amount;
        Currency = currency;
        CorrelationId = correlationId;
        Status = PaymentStatus.Pending;
        Provider = PaymentProvider.Stripe;
        CreatedAtUtc = createdAtUtc;
        CreatedBy = customerId.ToString();
    }

    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    public string CorrelationId { get; private set; }
    public PaymentStatus Status { get; private set; }
    public PaymentProvider Provider { get; private set; }
    public string? ProviderPaymentIntentId { get; private set; }
    public string? ClientSecret { get; private set; }
    public IReadOnlyCollection<PaymentAttempt> Attempts => _attempts.AsReadOnly();

    public static Payment Create(
        Guid orderId,
        Guid customerId,
        decimal amount,
        string currency,
        DateTimeOffset createdAtUtc,
        string correlationId = "")
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException("An order ID is required.", nameof(orderId));
        }

        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("A customer ID is required.", nameof(customerId));
        }

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), PaymentErrors.InvalidAmountDescription);
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        string normalizedCurrency = currency.Trim().ToUpperInvariant();
        if (normalizedCurrency.Length != 3)
        {
            throw new ArgumentException(PaymentErrors.UnsupportedCurrencyDescription, nameof(currency));
        }

        string normalizedCorrelationId = correlationId.Trim();
        if (normalizedCorrelationId.Length > 128)
        {
            throw new ArgumentException("The correlation ID is too long.", nameof(correlationId));
        }

        Payment payment = new(
            Guid.NewGuid(), orderId, customerId, amount, normalizedCurrency, createdAtUtc,
            normalizedCorrelationId);
        payment.RaiseDomainEvent(new PaymentCreatedDomainEvent(payment.Id, orderId, customerId));
        return payment;
    }

    public PaymentIntentUpdateOutcome SetPaymentIntent(
        string providerPaymentIntentId,
        string clientSecret,
        DateTimeOffset updatedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(providerPaymentIntentId))
        {
            return PaymentIntentUpdateOutcome.InvalidProviderReference;
        }

        if (string.IsNullOrWhiteSpace(clientSecret))
        {
            return PaymentIntentUpdateOutcome.InvalidClientSecret;
        }

        string providerReference = providerPaymentIntentId.Trim();
        if (ProviderPaymentIntentId is not null)
        {
            return PaymentIntentUpdateOutcome.AlreadyCreated;
        }

        ProviderPaymentIntentId = providerReference;
        ClientSecret = clientSecret.Trim();
        Status = PaymentStatus.RequiresPaymentMethod;
        _attempts.Add(new PaymentAttempt(
            Guid.NewGuid(), Id, providerReference, Status, updatedAtUtc, null, null));
        MarkUpdated(updatedAtUtc);
        RaiseDomainEvent(new PaymentIntentCreatedDomainEvent(
            Id, OrderId, CustomerId, providerReference));
        return PaymentIntentUpdateOutcome.Applied;
    }

    public void MarkIntentCreationFailed(
        string? failureCode,
        string? failureMessage,
        DateTimeOffset updatedAtUtc)
    {
        if (ProviderPaymentIntentId is not null)
        {
            return;
        }

        Status = PaymentStatus.Failed;
        _attempts.Add(new PaymentAttempt(
            Guid.NewGuid(), Id, null, Status, updatedAtUtc, failureCode, failureMessage));
        MarkUpdated(updatedAtUtc);
        RaiseDomainEvent(new PaymentIntentCreationFailedDomainEvent(
            Id, OrderId, CustomerId, failureCode));
    }

    public PaymentStateTransitionOutcome MarkProcessing(DateTimeOffset occurredAtUtc)
    {
        if (Status == PaymentStatus.Processing)
        {
            return PaymentStateTransitionOutcome.AlreadyApplied;
        }

        if (IsTerminal())
        {
            return PaymentStateTransitionOutcome.InvalidState;
        }

        Status = PaymentStatus.Processing;
        CurrentProviderAttempt()?.MarkProcessing();
        MarkUpdated(occurredAtUtc);
        RaiseDomainEvent(new PaymentProcessingDomainEvent(Id, OrderId, CustomerId));
        return PaymentStateTransitionOutcome.Applied;
    }

    public PaymentStateTransitionOutcome MarkSucceeded(DateTimeOffset occurredAtUtc)
    {
        if (Status == PaymentStatus.Succeeded)
        {
            return PaymentStateTransitionOutcome.AlreadyApplied;
        }

        if (Status is PaymentStatus.Failed or PaymentStatus.Cancelled)
        {
            return PaymentStateTransitionOutcome.InvalidState;
        }

        Status = PaymentStatus.Succeeded;
        ClientSecret = null;
        CurrentProviderAttempt()?.MarkSucceeded();
        MarkUpdated(occurredAtUtc);
        RaiseDomainEvent(new PaymentSucceededDomainEvent(Id, OrderId, CustomerId));
        return PaymentStateTransitionOutcome.Applied;
    }

    public PaymentStateTransitionOutcome MarkFailed(
        string? failureCode,
        string? failureMessage,
        DateTimeOffset occurredAtUtc)
    {
        if (Status == PaymentStatus.Failed)
        {
            return PaymentStateTransitionOutcome.AlreadyApplied;
        }

        if (Status is PaymentStatus.Succeeded or PaymentStatus.Cancelled)
        {
            return PaymentStateTransitionOutcome.InvalidState;
        }

        Status = PaymentStatus.Failed;
        ClientSecret = null;
        CurrentProviderAttempt()?.MarkFailed(failureCode, failureMessage);
        MarkUpdated(occurredAtUtc);
        RaiseDomainEvent(new PaymentFailedDomainEvent(
            Id, OrderId, CustomerId, NormalizeFailureCode(failureCode)));
        return PaymentStateTransitionOutcome.Applied;
    }

    private PaymentAttempt? CurrentProviderAttempt() =>
        _attempts.LastOrDefault(attempt =>
            attempt.ProviderReference == ProviderPaymentIntentId);

    private bool IsTerminal() =>
        Status is PaymentStatus.Succeeded or PaymentStatus.Failed or PaymentStatus.Cancelled;

    private static string? NormalizeFailureCode(string? failureCode)
    {
        if (string.IsNullOrWhiteSpace(failureCode))
        {
            return null;
        }

        string trimmed = failureCode.Trim();
        return trimmed.Length <= 100 ? trimmed : trimmed[..100];
    }

    private void MarkUpdated(DateTimeOffset updatedAtUtc)
    {
        UpdatedAtUtc = updatedAtUtc;
        UpdatedBy = CustomerId.ToString();
    }
}
