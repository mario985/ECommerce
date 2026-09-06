using ECommerce.Common.Domain.Events;

namespace ECommerce.Modules.Inventory.Domain.StockItems.Events;

public sealed record StockDecreasedDomainEvent(
    Guid StockItemId,
    Guid ProductId,
    int Quantity) : IDomainEvent;
