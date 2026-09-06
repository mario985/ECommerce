using ECommerce.Common.Application.Authentication;
using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Payments.Application.Abstractions;
using ECommerce.Modules.Payments.Domain.Payments;
using MediatR;

namespace ECommerce.Modules.Payments.Application.Payments.GetPayment;

public sealed class GetPaymentQueryHandler(
    IPaymentRepository paymentRepository,
    ICurrentUser currentUser)
    : IRequestHandler<GetPaymentQuery, Result<PaymentResponse>>
{
    public async Task<Result<PaymentResponse>> Handle(
        GetPaymentQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            return Failure(
                PaymentErrors.UnauthorizedCode,
                PaymentErrors.UnauthorizedDescription,
                ErrorType.Unauthorized);
        }

        Payment? payment = await paymentRepository.GetByOrderIdAsync(
            request.OrderId,
            cancellationToken);
        if (payment is null)
        {
            return Failure(
                PaymentErrors.NotFoundCode,
                PaymentErrors.NotFoundDescription,
                ErrorType.NotFound);
        }

        if (payment.CustomerId != currentUser.UserId.Value)
        {
            return Failure(
                PaymentErrors.ForbiddenCode,
                PaymentErrors.ForbiddenDescription,
                ErrorType.Forbidden);
        }

        return Result.Success(PaymentResponse.From(payment));
    }

    private static Result<PaymentResponse> Failure(
        string code,
        string description,
        ErrorType type) =>
        Result.Failure<PaymentResponse>(new Error(code, description, type));
}
