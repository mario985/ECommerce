namespace ECommerce.Modules.Inventory.Application;

public sealed class InventoryOptions
{
    public const string SectionName = "Inventory";
    public int LowStockThreshold { get; init; } = 10;
}
