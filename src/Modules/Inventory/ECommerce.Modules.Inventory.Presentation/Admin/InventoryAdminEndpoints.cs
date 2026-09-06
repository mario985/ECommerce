using ECommerce.Common.Application.Authentication;
using ECommerce.Common.Application.Errors;
using ECommerce.Common.Presentation.Results;
using ECommerce.Modules.Inventory.Application.Admin.SearchInventory;
using ECommerce.Modules.Inventory.Application.StockAdjustments.AdjustStock;
using ECommerce.Modules.Inventory.Application.StockAdjustments.GetStockAdjustmentHistory;
using ECommerce.Modules.Inventory.Domain.StockAdjustments;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Modules.Inventory.Presentation.Admin;

public static class InventoryAdminEndpoints
{
    public static RouteGroupBuilder MapInventoryAdminEndpoints(
        this RouteGroupBuilder group)
    {
        group.RequireAuthorization(AuthorizationPolicyNames.AdminOnly);
        group.MapGet("/", SearchAsync).WithName("InventoryAdmin.Search").WithTags("Inventory Admin");
        group.MapPost("/products/{productId:guid}/adjustments", AdjustAsync).WithName("InventoryAdmin.AdjustStock").WithTags("Inventory Admin");
        group.MapGet("/products/{productId:guid}/adjustments", HistoryAsync).WithName("InventoryAdmin.History").WithTags("Inventory Admin");
        return group;
    }

    private static async Task<IResult> AdjustAsync(
        Guid productId,
        AdjustStockRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        Result<AdjustStockResponse> result = await sender.Send(
            new AdjustStockCommand(
                productId, request.Quantity, request.Type, request.Reason, request.Note),
            cancellationToken);
        return result.IsFailure
            ? ApiResults.Problem(result.Error!)
            : Results.Created(
                $"/api/v1/admin/inventory/products/{productId}/adjustments/{result.Value.Id}",
                result.Value);
    }

    private static async Task<IResult> HistoryAsync(
        Guid productId,
        ISender sender,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20,
        StockAdjustmentType? type = null,
        StockAdjustmentReason? reason = null,
        DateTimeOffset? fromUtc = null,
        DateTimeOffset? toUtc = null)
    {
        Result<StockAdjustmentHistoryResponse> result = await sender.Send(
            new GetStockAdjustmentHistoryQuery(
                productId, page, pageSize, type, reason, fromUtc, toUtc),
            cancellationToken);
        return result.IsFailure ? ApiResults.Problem(result.Error!) : Results.Ok(result.Value);
    }

    private static async Task<IResult> SearchAsync(
        ISender sender,
        CancellationToken cancellationToken,
        string? search = null,
        string? sku = null,
        bool? lowStock = null,
        int? availableQuantityLessThan = null,
        int page = 1,
        int pageSize = 20)
    {
        Result<InventoryAdminResponse> result = await sender.Send(
            new SearchInventoryQuery(
                search, sku, lowStock, availableQuantityLessThan, page, pageSize),
            cancellationToken);
        return result.IsFailure ? ApiResults.Problem(result.Error!) : Results.Ok(result.Value);
    }
}
