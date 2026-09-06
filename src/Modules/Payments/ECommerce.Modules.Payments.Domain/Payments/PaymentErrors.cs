namespace ECommerce.Modules.Payments.Domain.Payments;

public static class PaymentErrors
{
    public const string NotFoundCode = "Payment.NotFound";
    public const string NotFoundDescription = "The payment could not be found.";
    public const string ForbiddenCode = "Payment.Forbidden";
    public const string ForbiddenDescription = "The payment belongs to another customer.";
    public const string UnauthorizedCode = "Payment.Unauthorized";
    public const string UnauthorizedDescription = "An authenticated customer is required.";
    public const string InvalidAmountCode = "Payment.InvalidAmount";
    public const string InvalidAmountDescription = "The payment amount must be greater than zero.";
    public const string UnsupportedCurrencyCode = "Payment.UnsupportedCurrency";
    public const string UnsupportedCurrencyDescription = "The payment currency is not supported.";
    public const string InvalidPrecisionCode = "Payment.InvalidPrecision";
    public const string InvalidPrecisionDescription = "The amount has more precision than its currency supports.";
    public const string AlreadyExistsForOrderCode = "Payment.AlreadyExistsForOrder";
    public const string AlreadyExistsForOrderDescription = "A payment already exists for this order.";
    public const string PaymentIntentAlreadyCreatedCode = "Payment.PaymentIntentAlreadyCreated";
    public const string PaymentIntentAlreadyCreatedDescription = "A provider payment intent has already been assigned.";
    public const string ProviderUnavailableCode = "Payment.ProviderUnavailable";
    public const string ProviderUnavailableDescription = "The payment provider is temporarily unavailable.";
    public const string ProviderRejectedRequestCode = "Payment.ProviderRejectedRequest";
    public const string ProviderRejectedRequestDescription = "The payment provider rejected the request.";
    public const string UnexpectedProviderFailureCode = "Payment.UnexpectedProviderFailure";
    public const string UnexpectedProviderFailureDescription = "The payment provider returned an unexpected failure.";
    public const string InvalidWebhookCode = "Payment.InvalidWebhook";
    public const string InvalidWebhookDescription = "The webhook signature or payload is invalid.";
    public const string WebhookProcessingFailedCode = "Payment.WebhookProcessingFailed";
    public const string WebhookProcessingFailedDescription = "The webhook could not be processed.";
}
