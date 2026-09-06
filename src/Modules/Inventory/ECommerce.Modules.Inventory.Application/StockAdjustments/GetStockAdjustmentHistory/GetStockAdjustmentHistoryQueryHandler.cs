using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Inventory.Domain.StockAdjustments;
using ECommerce.Modules.Inventory.Domain.StockItems;
using MediatR;

namespace ECommerce.Modules.Inventory.Application.StockAdjustments.GetStockAdjustmentHistory;

public sealed class GetStockAdjustmentHistoryQueryHandler(
    IStockItemRepository stockItemRepository,
    IStockAdjustmentRepository stockAdjustmentRepository,
    GetStockAdjustmentHistoryValidator validator)
    : IRequestHandler<GetStockAdjustmentHistoryQuery, Result<StockAdjustmentHistoryResponse>>
{
    public async Task<Result<StockAdjustmentHistoryResponse>> Handle(
        GetStockAdjustmentHistoryQuery request,
        CancellationToken cancellationToken)
    {
        if (!(await validator.ValidateAsync(request, cancellationToken)).IsValid)
        {
            return Result.Failure<StockAdjustmentHistoryResponse>(new Error(
                InventoryErrors.InvalidAdjustment,
                "The adjustment history filters are invalid.",
                ErrorType.Validation));
        }

        if (!await stockItemRepository.ExistsByProductIdAsync(request.ProductId, cancellationToken))
        {
            return Result.Failure<StockAdjustmentHistoryResponse>(new Error(
                InventoryErrors.StockItemNotFound,
                "The stock item was not found.",
                ErrorType.NotFound));
        }

        StockAdjustmentSearchResult result = await stockAdjustmentRepository.SearchAsync(
            request.ProductId, request.Type, request.Reason, request.FromUtc, request.ToUtc,
            request.Page, request.PageSize, cancellationToken);
        StockAdjustmentResponse[] items = result.Items.Select(adjustment => new StockAdjustmentResponse(
            adjustment.Id,
            adjustment.Quantity,
            adjustment.Type,
            adjustment.Reason,
            adjustment.Note,
            adjustment.PerformedBy,
            adjustment.OccurredAtUtc)).ToArray();
        int totalPages = result.TotalCount == 0
            ? 0
            : (int)Math.Ceiling(result.TotalCount / (double)request.PageSize);

        return Result.Success(new StockAdjustmentHistoryResponse(
            items, request.Page, request.PageSize, result.TotalCount, totalPages));
    }
}
