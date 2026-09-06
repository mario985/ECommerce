using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Inventory.Application;

internal static partial class InventoryAdminLog
{
    [LoggerMessage(3101, LogLevel.Information,
        "Stock {AdjustmentType}; AdminUserId={AdminUserId}, ProductId={ProductId}, StockItemId={StockItemId}, AdjustmentId={AdjustmentId}, Quantity={Quantity}, Reason={Reason}")]
    public static partial void StockAdjusted(
        ILogger logger,
        object adjustmentType,
        Guid adminUserId,
        Guid productId,
        Guid stockItemId,
        Guid adjustmentId,
        int quantity,
        object reason);

    [LoggerMessage(3102, LogLevel.Information,
        "Low-stock inventory queried; Threshold={LowStockThreshold}, ResultCount={ResultCount}")]
    public static partial void LowStockQueried(
        ILogger logger,
        int lowStockThreshold,
        long resultCount);
}
