using ECommerce.Modules.Inventory.Domain.StockItems;
using MediatR;

namespace ECommerce.Modules.Inventory.Application.StockItems.GetStock;

public sealed class GetStockQueryHandler(IStockItemRepository stockItemRepository)
    : IRequestHandler<GetStockQuery, StockDetails?>
{
    public async Task<StockDetails?> Handle(
        GetStockQuery request,
        CancellationToken cancellationToken)
    {
        StockItem? stockItem = await stockItemRepository.GetByProductIdAsync(
            request.ProductId,
            cancellationToken);

        return stockItem is null
            ? null
            : new StockDetails(
                stockItem.ProductId,
                stockItem.Sku,
                stockItem.AvailableQuantity,
                stockItem.ReservedQuantity);
    }
}
