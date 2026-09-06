namespace ECommerce.Modules.Payments.Infrastructure.Stripe;

public sealed class StripeOptions
{
    public const string SectionName = "Stripe";

    public bool Enabled { get; init; }
    public string SecretKey { get; init; } = string.Empty;
    public string WebhookSecret { get; init; } = string.Empty;
}
