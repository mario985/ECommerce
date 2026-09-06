namespace ECommerce.Modules.Inventory.Application.Admin.SearchInventory;

public sealed record InventoryAdminItemResponse(
    Guid ProductId,
    string Sku,
    int AvailableQuantity,
    int ReservedQuantity,
    int TotalQuantity,
    bool IsLowStock);

public sealed record InventoryAdminResponse(
    IReadOnlyCollection<InventoryAdminItemResponse> Items,
    int Page,
    int PageSize,
    long TotalCount,
    int TotalPages);
