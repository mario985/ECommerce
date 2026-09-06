using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Inventory.Domain.StockAdjustments;
using MediatR;

namespace ECommerce.Modules.Inventory.Application.StockAdjustments.AdjustStock;

public sealed record AdjustStockCommand(
    Guid ProductId,
    int Quantity,
    StockAdjustmentType Type,
    StockAdjustmentReason Reason,
    string? Note) : IRequest<Result<AdjustStockResponse>>;
