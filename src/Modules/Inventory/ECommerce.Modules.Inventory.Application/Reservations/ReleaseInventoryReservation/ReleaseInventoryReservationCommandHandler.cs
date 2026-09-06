using ECommerce.Common.Application.Messaging;
using ECommerce.Common.Application.Observability;
using ECommerce.Modules.Inventory.Application.Reservations;
using ECommerce.Modules.Inventory.Contracts.IntegrationEvents;
using ECommerce.Modules.Inventory.Domain.Reservations;
using ECommerce.Modules.Inventory.Domain.StockItems;
using MediatR;

namespace ECommerce.Modules.Inventory.Application.Reservations.ReleaseInventoryReservation;

public sealed class ReleaseInventoryReservationCommandHandler(
    IStockItemRepository stockItemRepository,
    IIntegrationEventPublisher integrationEventPublisher)
    : IRequestHandler<ReleaseInventoryReservationCommand>
{
    public async Task Handle(
        ReleaseInventoryReservationCommand request,
        CancellationToken cancellationToken)
    {
        using System.Diagnostics.Activity? activity = CommerceActivitySources.Inventory.StartActivity("Inventory.ReleaseReservation");
        activity?.SetTag("ecommerce.reservation.id", request.ReservationId);
        StockItem stockItem = await stockItemRepository.GetByReservationIdAsync(
                request.ReservationId,
                cancellationToken)
            ?? throw new ReservationNotFoundException(request.ReservationId);

        InventoryReservation reservation;

        try
        {
            reservation = stockItem.ReleaseReservation(request.ReservationId);
        }
        catch (InvalidReservationTransitionException exception)
        {
            throw new ReservationConflictException(exception.Message, exception);
        }

        await stockItemRepository.UpdateAsync(stockItem, cancellationToken);

        await integrationEventPublisher.PublishAsync(
            new ProductAvailabilityChangedIntegrationEvent(
                Guid.NewGuid(),
                stockItem.ProductId,
                stockItem.AvailableQuantity > 0,
                DateTimeOffset.UtcNow),
            cancellationToken);

        await integrationEventPublisher.PublishAsync(
            new InventoryReservationReleasedIntegrationEvent(
                Guid.NewGuid(),
                reservation.Id,
                reservation.ProductId,
                reservation.Quantity,
                DateTimeOffset.UtcNow),
            cancellationToken);
    }
}
