using ECommerce.Common.Application.Errors;
using MediatR;

namespace ECommerce.Modules.Payments.Application.Payments.GetPayment;

public sealed record GetPaymentQuery(Guid OrderId) : IRequest<Result<PaymentResponse>>;
