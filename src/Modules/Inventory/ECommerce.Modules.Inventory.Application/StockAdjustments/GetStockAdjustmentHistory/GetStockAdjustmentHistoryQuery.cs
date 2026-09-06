using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Inventory.Domain.StockAdjustments;
using MediatR;

namespace ECommerce.Modules.Inventory.Application.StockAdjustments.GetStockAdjustmentHistory;

public sealed record GetStockAdjustmentHistoryQuery(
    Guid ProductId,
    int Page,
    int PageSize,
    StockAdjustmentType? Type,
    StockAdjustmentReason? Reason,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc) : IRequest<Result<StockAdjustmentHistoryResponse>>;
