using ECommerce.Modules.Inventory.Application.Reservations;
using ECommerce.Modules.Inventory.Application.Reservations.ConfirmInventoryReservation;
using ECommerce.Modules.Inventory.Application.Reservations.ReleaseInventoryReservation;
using ECommerce.Modules.Inventory.Application.Reservations.ReserveInventory;
using ECommerce.Modules.Inventory.Application.StockItems.GetStock;
using ECommerce.Modules.Inventory.Application.StockItems;
using ECommerce.Modules.Inventory.Presentation.Admin;
using ECommerce.Modules.Inventory.Presentation.Reservations;
using ECommerce.Modules.Inventory.Presentation.StockItems;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ECommerce.Common.Presentation;
using ECommerce.Common.Application.Errors;
using ECommerce.Common.Presentation.Results;

namespace ECommerce.Modules.Inventory.Presentation;

public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder productGroup = endpoints.MapGroup("/api/v1/inventory/products")
            .RequireRateLimiting(ECommerce.Common.Presentation.RateLimitingPolicyNames.Authenticated);
        RouteGroupBuilder reservationGroup = endpoints.MapGroup("/api/v1/inventory/reservations")
            .RequireRateLimiting(ECommerce.Common.Presentation.RateLimitingPolicyNames.Authenticated);

        productGroup.MapGet("/{productId:guid}", GetStockAsync).WithName("Inventory.GetStock").WithTags("Inventory");
        productGroup.MapPost("/{productId:guid}/reservations", ReserveInventoryAsync).WithName("Inventory.Reserve").WithTags("Inventory");

        reservationGroup.MapPost("/{reservationId:guid}/release", ReleaseReservationAsync).WithName("Inventory.ReleaseReservation").WithTags("Inventory");
        reservationGroup.MapPost("/{reservationId:guid}/confirm", ConfirmReservationAsync).WithName("Inventory.ConfirmReservation").WithTags("Inventory");

        endpoints.MapGroup("/api/v1/admin/inventory")
            .RequireRateLimiting(ECommerce.Common.Presentation.RateLimitingPolicyNames.Admin)
            .MapInventoryAdminEndpoints();

        return endpoints;
    }

    private static async Task<IResult> ReserveInventoryAsync(
        Guid productId,
        ReserveInventoryRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        try
        {
            Guid reservationId = await sender.Send(
                new ReserveInventoryCommand(productId, request.Quantity),
                cancellationToken);

            return Results.Created(
                $"/api/v1/inventory/reservations/{reservationId}",
                new ReservationCreatedResponse(reservationId));
        }
        catch (StockItemNotFoundException)
        {
            return ApiResults.Problem(new Error(
                "Inventory.StockItemNotFound", "The requested stock item was not found.", ErrorType.NotFound));
        }
        catch (InvalidReservationRequestException)
        {
            return ApiResults.Problem(new Error(
                "Inventory.InvalidReservation", "The reservation request is invalid.", ErrorType.Validation));
        }
    }

    private static Task<IResult> ReleaseReservationAsync(
        Guid reservationId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        return ProcessReservationAsync(
            new ReleaseInventoryReservationCommand(reservationId),
            sender,
            cancellationToken);
    }

    private static Task<IResult> ConfirmReservationAsync(
        Guid reservationId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        return ProcessReservationAsync(
            new ConfirmInventoryReservationCommand(reservationId),
            sender,
            cancellationToken);
    }

    private static async Task<IResult> ProcessReservationAsync(
        IRequest command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        try
        {
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }
        catch (ReservationNotFoundException)
        {
            return ApiResults.Problem(new Error(
                "Inventory.ReservationNotFound", "The requested reservation was not found.", ErrorType.NotFound));
        }
        catch (ReservationConflictException)
        {
            return ApiResults.Problem(new Error(
                "Inventory.ReservationConflict", "The reservation is not in a state that permits this operation.", ErrorType.Conflict));
        }
    }

    private static async Task<IResult> GetStockAsync(
        Guid productId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        StockDetails? stock = await sender.Send(
            new GetStockQuery(productId),
            cancellationToken);

        return stock is null
            ? ApiResults.Problem(new Error(
                "Inventory.StockItemNotFound", "The requested stock item was not found.", ErrorType.NotFound))
            : Results.Ok(new StockResponse(
                stock.ProductId,
                stock.Sku,
                stock.AvailableQuantity,
                stock.ReservedQuantity));
    }

}
