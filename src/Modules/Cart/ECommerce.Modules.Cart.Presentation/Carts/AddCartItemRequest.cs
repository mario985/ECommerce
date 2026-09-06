namespace ECommerce.Modules.Cart.Presentation.Carts;

public sealed record AddCartItemRequest(Guid ProductId, int Quantity);
