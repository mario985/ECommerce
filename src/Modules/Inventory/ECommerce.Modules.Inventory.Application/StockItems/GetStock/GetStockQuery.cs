using MediatR;

namespace ECommerce.Modules.Inventory.Application.StockItems.GetStock;

public sealed record GetStockQuery(Guid ProductId) : IRequest<StockDetails?>;
