namespace ECommerce.Modules.Inventory.Presentation.StockItems;

public sealed record StockResponse(
    Guid ProductId,
    string Sku,
    int AvailableQuantity,
    int ReservedQuantity);
