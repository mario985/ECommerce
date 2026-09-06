namespace ECommerce.Modules.Inventory.Domain.StockItems;

public sealed class InsufficientAvailableStockException(
    int availableQuantity,
    int requestedQuantity)
    : Exception(
        $"Cannot decrease stock by {requestedQuantity}; only {availableQuantity} is available.")
{
    public int AvailableQuantity { get; } = availableQuantity;

    public int RequestedQuantity { get; } = requestedQuantity;
}
