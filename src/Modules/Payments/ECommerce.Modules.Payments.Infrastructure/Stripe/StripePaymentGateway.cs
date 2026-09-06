using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Payments.Application.Abstractions;
using ECommerce.Modules.Payments.Domain.Payments;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace ECommerce.Modules.Payments.Infrastructure.Stripe;

internal sealed class StripePaymentGateway(
    IStripeAmountConverter amountConverter,
    IOptions<StripeOptions> options,
    ILogger<StripePaymentGateway> logger) : IPaymentGateway
{
    public async Task<Result<CreatePaymentIntentResult>> CreatePaymentIntentAsync(
        CreatePaymentIntentRequest request,
        CancellationToken cancellationToken)
    {
        Result<long> amountResult = amountConverter.ToMinorUnits(request.Amount, request.Currency);
        if (amountResult.IsFailure)
        {
            return Result.Failure<CreatePaymentIntentResult>(amountResult.Error!);
        }

        StripeOptions stripeOptions = options.Value;
        if (!stripeOptions.Enabled || string.IsNullOrWhiteSpace(stripeOptions.SecretKey))
        {
            return Result.Failure<CreatePaymentIntentResult>(new Error(
                PaymentErrors.ProviderUnavailableCode,
                PaymentErrors.ProviderUnavailableDescription,
                ErrorType.Failure));
        }

        try
        {
            StripeClient client = new(stripeOptions.SecretKey);
            PaymentIntentService service = new(client);
            PaymentIntent intent = await service.CreateAsync(
                new PaymentIntentCreateOptions
                {
                    Amount = amountResult.Value,
                    Currency = request.Currency.Trim().ToLowerInvariant(),
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true,
                    },
                    Metadata = new Dictionary<string, string>
                    {
                        ["paymentId"] = request.PaymentId.ToString(),
                        ["orderId"] = request.OrderId.ToString(),
                        ["customerId"] = request.CustomerId.ToString(),
                    },
                },
                new RequestOptions { IdempotencyKey = request.IdempotencyKey },
                cancellationToken);

            if (string.IsNullOrWhiteSpace(intent.Id) ||
                string.IsNullOrWhiteSpace(intent.ClientSecret))
            {
                return Result.Failure<CreatePaymentIntentResult>(
                    StripeExceptionMapper.Unexpected());
            }

            return Result.Success(new CreatePaymentIntentResult(
                intent.Id,
                intent.ClientSecret,
                intent.Status));
        }
        catch (StripeException exception)
        {
            StripeLog.RequestFailed(
                logger,
                null,
                (int)exception.HttpStatusCode,
                exception.StripeError?.Code);
            return Result.Failure<CreatePaymentIntentResult>(
                StripeExceptionMapper.Map(exception));
        }
        catch (HttpRequestException exception)
        {
            StripeLog.UnexpectedFailure(logger, exception);
            return Result.Failure<CreatePaymentIntentResult>(new Error(
                PaymentErrors.ProviderUnavailableCode,
                PaymentErrors.ProviderUnavailableDescription,
                ErrorType.Failure));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            StripeLog.UnexpectedFailure(logger, exception);
            return Result.Failure<CreatePaymentIntentResult>(
                StripeExceptionMapper.Unexpected());
        }
    }
}
