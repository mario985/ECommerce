using ECommerce.Common.Application.Errors;

namespace ECommerce.Modules.Payments.Application.Abstractions;

public interface IPaymentGateway
{
    Task<Result<CreatePaymentIntentResult>> CreatePaymentIntentAsync(
        CreatePaymentIntentRequest request,
        CancellationToken cancellationToken);
}

public sealed record CreatePaymentIntentRequest(
    Guid PaymentId,
    Guid OrderId,
    Guid CustomerId,
    decimal Amount,
    string Currency,
    string IdempotencyKey);

public sealed record CreatePaymentIntentResult(
    string ProviderPaymentIntentId,
    string ClientSecret,
    string ProviderStatus);
