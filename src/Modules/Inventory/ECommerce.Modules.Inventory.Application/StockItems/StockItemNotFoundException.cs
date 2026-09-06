namespace ECommerce.Modules.Inventory.Application.StockItems;

public sealed class StockItemNotFoundException(Guid productId)
    : Exception($"Stock for product '{productId}' was not found.")
{
    public Guid ProductId { get; } = productId;
}
