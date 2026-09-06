using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Inventory.Domain.StockItems;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Inventory.Application.Admin.SearchInventory;

public sealed class SearchInventoryQueryHandler(
    IInventoryAdminRepository repository,
    SearchInventoryValidator validator,
    InventoryOptions options,
    ILogger<SearchInventoryQueryHandler> logger)
    : IRequestHandler<SearchInventoryQuery, Result<InventoryAdminResponse>>
{
    public async Task<Result<InventoryAdminResponse>> Handle(
        SearchInventoryQuery request,
        CancellationToken cancellationToken)
    {
        if (!(await validator.ValidateAsync(request, cancellationToken)).IsValid)
        {
            return Result.Failure<InventoryAdminResponse>(new Error(
                InventoryErrors.InvalidAdjustment,
                "The inventory search filters are invalid.",
                ErrorType.Validation));
        }

        int threshold = options.LowStockThreshold;
        StockItemSearchResult result = await repository.SearchAsync(
            request.Search, request.Sku, request.LowStock, threshold,
            request.AvailableQuantityLessThan, request.Page, request.PageSize,
            cancellationToken);
        InventoryAdminItemResponse[] items = result.Items.Select(item =>
            new InventoryAdminItemResponse(
                item.ProductId,
                item.Sku,
                item.AvailableQuantity,
                item.ReservedQuantity,
                checked(item.AvailableQuantity + item.ReservedQuantity),
                item.AvailableQuantity <= threshold)).ToArray();
        int totalPages = result.TotalCount == 0
            ? 0
            : (int)Math.Ceiling(result.TotalCount / (double)request.PageSize);

        if (request.LowStock == true)
        {
            InventoryAdminLog.LowStockQueried(logger, threshold, result.TotalCount);
        }

        return Result.Success(new InventoryAdminResponse(
            items, request.Page, request.PageSize, result.TotalCount, totalPages));
    }
}
