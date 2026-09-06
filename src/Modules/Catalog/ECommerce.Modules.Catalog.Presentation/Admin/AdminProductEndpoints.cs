using ECommerce.Common.Application.Authentication;
using ECommerce.Modules.Catalog.Application.Products.ActivateProduct;
using ECommerce.Modules.Catalog.Application.Products.CreateProduct;
using ECommerce.Modules.Catalog.Application.Products.DeactivateProduct;
using ECommerce.Modules.Catalog.Application.Products.DeleteProduct;
using ECommerce.Modules.Catalog.Application.Products.SearchProducts;
using ECommerce.Modules.Catalog.Application.Products.UpdateProduct;
using ECommerce.Modules.Catalog.Application.Products;
using ECommerce.Modules.Catalog.Presentation.Products;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using ECommerce.Common.Application.Errors;
using ECommerce.Common.Presentation.Results;

namespace ECommerce.Modules.Catalog.Presentation.Admin;

public static class AdminProductEndpoints
{
    public static RouteGroupBuilder MapAdminProductEndpoints(this RouteGroupBuilder group)
    {
        group.RequireAuthorization(AuthorizationPolicyNames.AdminOnly);
        group.MapGet("/", SearchAsync).WithName("CatalogAdmin.SearchProducts").WithTags("Catalog Admin");
        group.MapPost("/", CreateAsync).WithName("CatalogAdmin.CreateProduct").WithTags("Catalog Admin");
        group.MapPut("/{id:guid}", UpdateAsync).WithName("CatalogAdmin.UpdateProduct").WithTags("Catalog Admin");
        group.MapPatch("/{id:guid}/activate", ActivateAsync).WithName("CatalogAdmin.ActivateProduct").WithTags("Catalog Admin");
        group.MapPatch("/{id:guid}/deactivate", DeactivateAsync).WithName("CatalogAdmin.DeactivateProduct").WithTags("Catalog Admin");
        group.MapDelete("/{id:guid}", DeleteAsync).WithName("CatalogAdmin.DeleteProduct").WithTags("Catalog Admin");
        return group;
    }

    private static async Task<IResult> SearchAsync(
        ISender sender,
        CancellationToken cancellationToken,
        string? search = null,
        string? sku = null,
        bool? isActive = null,
        int page = 1,
        int pageSize = 20)
    {
        Result<SearchProductsResponse> result = await sender.Send(
            new SearchProductsQuery(
                Query: search,
                Sort: "name",
                Page: page,
                PageSize: pageSize,
                Sku: sku,
                IsActive: isActive,
                IncludeSkuAndCategoryInTextSearch: false),
            cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ApiResults.Problem(result.Error!);
    }

    private static async Task<IResult> CreateAsync(
        CreateProductRequest request,
        ISender sender,
        ICurrentUser currentUser,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        try
        {
            Guid id = await sender.Send(new CreateProductCommand(
                request.Name, request.Description, request.Sku, request.Price, request.Currency, request.CategoryId),
                cancellationToken);
            Log(loggerFactory, "created", currentUser.UserId, id);
            return Results.Created($"/api/v1/catalog/products/{id}", new { Id = id });
        }
        catch (DuplicateProductSkuException)
        {
            return ApiResults.Problem(new Error("Catalog.DuplicateSku", "The product SKU is already in use.", ErrorType.Conflict));
        }
        catch (CategoryNotFoundException)
        {
            return ApiResults.Problem(new Error("Catalog.CategoryNotFound", "The requested category was not found.", ErrorType.NotFound));
        }
        catch (ArgumentException)
        {
            return ApiResults.Problem(new Error("Catalog.InvalidProduct", "The product data is invalid.", ErrorType.Validation));
        }
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateProductRequest request,
        ISender sender,
        ICurrentUser currentUser,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        try
        {
            await sender.Send(new UpdateProductCommand(
                id, request.Name, request.Description, request.Sku, request.Price, request.Currency, request.CategoryId),
                cancellationToken);
            Log(loggerFactory, "updated", currentUser.UserId, id);
            return Results.NoContent();
        }
        catch (ProductNotFoundException)
        {
            return ApiResults.Problem(new Error("Catalog.ProductNotFound", "The requested product was not found.", ErrorType.NotFound));
        }
        catch (DuplicateProductSkuException)
        {
            return ApiResults.Problem(new Error("Catalog.DuplicateSku", "The product SKU is already in use.", ErrorType.Conflict));
        }
        catch (CategoryNotFoundException)
        {
            return ApiResults.Problem(new Error("Catalog.CategoryNotFound", "The requested category was not found.", ErrorType.NotFound));
        }
        catch (ArgumentException)
        {
            return ApiResults.Problem(new Error("Catalog.InvalidProduct", "The product data is invalid.", ErrorType.Validation));
        }
    }

    private static Task<IResult> ActivateAsync(
        Guid id, ISender sender, ICurrentUser currentUser, ILoggerFactory loggerFactory,
        CancellationToken cancellationToken) =>
        LifecycleAsync(new ActivateProductCommand(id), "activated", id, sender,
            currentUser, loggerFactory, cancellationToken);

    private static Task<IResult> DeactivateAsync(
        Guid id, ISender sender, ICurrentUser currentUser, ILoggerFactory loggerFactory,
        CancellationToken cancellationToken) =>
        LifecycleAsync(new DeactivateProductCommand(id), "deactivated", id, sender,
            currentUser, loggerFactory, cancellationToken);

    private static Task<IResult> DeleteAsync(
        Guid id, ISender sender, ICurrentUser currentUser, ILoggerFactory loggerFactory,
        CancellationToken cancellationToken) =>
        LifecycleAsync(new DeleteProductCommand(id), "deleted", id, sender,
            currentUser, loggerFactory, cancellationToken);

    private static async Task<IResult> LifecycleAsync(
        IRequest command,
        string operation,
        Guid productId,
        ISender sender,
        ICurrentUser currentUser,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        try
        {
            await sender.Send(command, cancellationToken);
            Log(loggerFactory, operation, currentUser.UserId, productId);
            return Results.NoContent();
        }
        catch (ProductNotFoundException)
        {
            return ApiResults.Problem(new Error("Catalog.ProductNotFound", "The requested product was not found.", ErrorType.NotFound));
        }
    }

    private static void Log(
        ILoggerFactory loggerFactory,
        string operation,
        Guid? adminUserId,
        Guid productId)
    {
        CatalogAdminLog.ProductMutated(
            loggerFactory.CreateLogger("Catalog.Admin"), operation, adminUserId, productId);
    }
}
