using ECommerce.Common.Application.Errors;
using MediatR;

namespace ECommerce.Modules.Payments.Application.Webhooks.ProcessStripeWebhook;

public sealed record ProcessStripeWebhookCommand(
    string Payload,
    string Signature) : IRequest<Result<StripeWebhookResult>>;
