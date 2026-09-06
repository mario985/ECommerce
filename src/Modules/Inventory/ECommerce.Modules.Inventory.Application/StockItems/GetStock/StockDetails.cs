namespace ECommerce.Modules.Inventory.Application.StockItems.GetStock;

public sealed record StockDetails(
    Guid ProductId,
    string Sku,
    int AvailableQuantity,
    int ReservedQuantity);
