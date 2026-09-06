using ECommerce.Common.Application.Authentication;
using ECommerce.Common.Application.Errors;
using ECommerce.Common.Application.Messaging;
using ECommerce.Modules.Inventory.Contracts.IntegrationEvents;
using ECommerce.Modules.Inventory.Domain.StockAdjustments;
using ECommerce.Modules.Inventory.Domain.StockItems;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Inventory.Application.StockAdjustments.AdjustStock;

public sealed class AdjustStockCommandHandler(
    IStockItemRepository stockItemRepository,
    IStockAdjustmentRepository stockAdjustmentRepository,
    ICurrentUser currentUser,
    AdjustStockValidator validator,
    IIntegrationEventPublisher integrationEventPublisher,
    TimeProvider timeProvider,
    ILogger<AdjustStockCommandHandler> logger)
    : IRequestHandler<AdjustStockCommand, Result<AdjustStockResponse>>
{
    public async Task<Result<AdjustStockResponse>> Handle(
        AdjustStockCommand request,
        CancellationToken cancellationToken)
    {
        ValidationResult validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            string code = Enum.IsDefined(request.Reason)
                ? InventoryErrors.InvalidAdjustment
                : InventoryErrors.InvalidAdjustmentReason;
            return Failure(code, validation.Errors[0].ErrorMessage, ErrorType.Validation);
        }

        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            return Failure(
                InventoryErrors.Unauthorized,
                "An authenticated administrator is required.",
                ErrorType.Unauthorized);
        }

        StockItem? stockItem = await stockItemRepository.GetByProductIdAsync(
            request.ProductId, cancellationToken);
        if (stockItem is null)
        {
            return Failure(
                InventoryErrors.StockItemNotFound,
                "The stock item was not found.",
                ErrorType.NotFound);
        }

        try
        {
            if (request.Type == StockAdjustmentType.Increase)
            {
                stockItem.IncreaseStock(request.Quantity);
            }
            else
            {
                stockItem.DecreaseStock(request.Quantity);
            }
        }
        catch (InsufficientAvailableStockException exception)
        {
            return Failure(
                InventoryErrors.InsufficientAvailableStock,
                exception.Message,
                ErrorType.Validation);
        }
        catch (OverflowException exception)
        {
            return Failure(
                InventoryErrors.InvalidAdjustment,
                exception.Message,
                ErrorType.Validation);
        }

        DateTimeOffset occurredAtUtc = timeProvider.GetUtcNow();
        StockAdjustment adjustment = StockAdjustment.Create(
            stockItem.Id,
            stockItem.ProductId,
            request.Quantity,
            request.Type,
            request.Reason,
            request.Note,
            currentUser.UserId.Value,
            occurredAtUtc);

        await stockAdjustmentRepository.AddAsync(adjustment, cancellationToken);
        await stockItemRepository.SaveChangesAsync(cancellationToken);
        await integrationEventPublisher.PublishAsync(
            new ProductAvailabilityChangedIntegrationEvent(
                Guid.NewGuid(),
                stockItem.ProductId,
                stockItem.AvailableQuantity > 0,
                occurredAtUtc),
            cancellationToken);

        InventoryAdminLog.StockAdjusted(
            logger,
            adjustment.Type,
            adjustment.PerformedBy,
            adjustment.ProductId,
            adjustment.StockItemId,
            adjustment.Id,
            adjustment.Quantity,
            adjustment.Reason);

        return Result.Success(new AdjustStockResponse(
            adjustment.Id,
            adjustment.ProductId,
            adjustment.Quantity,
            adjustment.Type,
            adjustment.Reason,
            adjustment.Note,
            stockItem.AvailableQuantity,
            stockItem.ReservedQuantity,
            adjustment.PerformedBy,
            adjustment.OccurredAtUtc));
    }

    private static Result<AdjustStockResponse> Failure(
        string code,
        string description,
        ErrorType type) =>
        Result.Failure<AdjustStockResponse>(new Error(code, description, type));
}
