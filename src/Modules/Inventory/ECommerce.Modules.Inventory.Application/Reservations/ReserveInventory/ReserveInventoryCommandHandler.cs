using ECommerce.Common.Application.Messaging;
using ECommerce.Modules.Inventory.Application.Reservations;
using ECommerce.Modules.Inventory.Application.StockItems;
using ECommerce.Modules.Inventory.Contracts.IntegrationEvents;
using ECommerce.Modules.Inventory.Domain.Reservations;
using ECommerce.Modules.Inventory.Domain.StockItems;
using MediatR;

namespace ECommerce.Modules.Inventory.Application.Reservations.ReserveInventory;

public sealed class ReserveInventoryCommandHandler(
    IStockItemRepository stockItemRepository,
    IIntegrationEventPublisher integrationEventPublisher)
    : IRequestHandler<ReserveInventoryCommand, Guid>
{
    public async Task<Guid> Handle(
        ReserveInventoryCommand request,
        CancellationToken cancellationToken)
    {
        StockItem stockItem = await stockItemRepository.GetByProductIdAsync(
                request.ProductId,
                cancellationToken)
            ?? throw new StockItemNotFoundException(request.ProductId);

        DateTimeOffset occurredAtUtc = DateTimeOffset.UtcNow;
        InventoryReservation reservation;

        try
        {
            reservation = stockItem.Reserve(request.Quantity, occurredAtUtc);
        }
        catch (InsufficientAvailableStockException exception)
        {
            await integrationEventPublisher.PublishAsync(
                new InventoryReservationFailedIntegrationEvent(
                    Guid.NewGuid(),
                    request.ProductId,
                    request.Quantity,
                    exception.Message,
                    occurredAtUtc),
                cancellationToken);

            throw new InvalidReservationRequestException(
                exception.Message,
                exception);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            throw new InvalidReservationRequestException(
                exception.Message,
                exception);
        }

        await stockItemRepository.UpdateAsync(stockItem, cancellationToken);

        await integrationEventPublisher.PublishAsync(
            new ProductAvailabilityChangedIntegrationEvent(
                Guid.NewGuid(),
                stockItem.ProductId,
                stockItem.AvailableQuantity > 0,
                occurredAtUtc),
            cancellationToken);

        await integrationEventPublisher.PublishAsync(
            new InventoryReservedIntegrationEvent(
                Guid.NewGuid(),
                reservation.Id,
                reservation.ProductId,
                reservation.Quantity,
                occurredAtUtc),
            cancellationToken);

        return reservation.Id;
    }
}
