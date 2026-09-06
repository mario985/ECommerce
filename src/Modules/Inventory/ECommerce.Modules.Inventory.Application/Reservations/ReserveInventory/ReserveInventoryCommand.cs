using MediatR;

namespace ECommerce.Modules.Inventory.Application.Reservations.ReserveInventory;

public sealed record ReserveInventoryCommand(
    Guid ProductId,
    int Quantity) : IRequest<Guid>;
