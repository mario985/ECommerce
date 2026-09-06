namespace ECommerce.Modules.Cart.Application.Checkouts.GetCheckout;

public sealed record CartCheckoutResponse(
    Guid CheckoutId,
    Guid? OrderId,
    string Status,
    IReadOnlyCollection<CartCheckoutItemResponse> Items,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset? FailedAtUtc);

public sealed record CartCheckoutItemResponse(
    Guid ProductId,
    string ProductName,
    string Sku,
    decimal UnitPrice,
    string Currency,
    int Quantity);
