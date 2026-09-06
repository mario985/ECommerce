namespace ECommerce.Modules.Inventory.Domain.StockAdjustments;

public enum StockAdjustmentReason
{
    Restock = 1,
    Correction = 2,
    Damaged = 3,
    Returned = 4,
    Lost = 5,
    ManualAdjustment = 6,
}
