using ECommerce.Modules.Catalog.Contracts.IntegrationEvents;
using ECommerce.Modules.Inventory.Domain.StockItems;
using MediatR;

namespace ECommerce.Modules.Inventory.Application.StockItems.CreateStockFromProduct;

public sealed class CreateStockItemOnProductCreatedHandler(
    IStockItemRepository stockItemRepository)
    : INotificationHandler<ProductCreatedIntegrationEvent>
{
    public async Task Handle(
        ProductCreatedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        if (await stockItemRepository.ExistsByProductIdAsync(
                notification.ProductId,
                cancellationToken))
        {
            return;
        }

        StockItem stockItem = StockItem.Create(
            notification.ProductId,
            notification.Sku);

        await stockItemRepository.AddAsync(stockItem, cancellationToken);
    }
}
