namespace ECommerce.Modules.Payments.Application.Webhooks.ProcessStripeWebhook;

public sealed record StripeWebhookResult(
    string Outcome,
    string? StripeEventId);
