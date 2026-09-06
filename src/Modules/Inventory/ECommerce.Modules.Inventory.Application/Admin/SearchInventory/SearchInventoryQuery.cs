using ECommerce.Common.Application.Errors;
using MediatR;

namespace ECommerce.Modules.Inventory.Application.Admin.SearchInventory;

public sealed record SearchInventoryQuery(
    string? Search,
    string? Sku,
    bool? LowStock,
    int? AvailableQuantityLessThan,
    int Page,
    int PageSize) : IRequest<Result<InventoryAdminResponse>>;
