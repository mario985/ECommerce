using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Catalog.Presentation.Admin;

internal static partial class CatalogAdminLog
{
    [LoggerMessage(8301, LogLevel.Information,
        "Admin Product {Operation}; AdminUserId={AdminUserId}, ProductId={ProductId}")]
    public static partial void ProductMutated(
        ILogger logger,
        string operation,
        Guid? adminUserId,
        Guid productId);
}
