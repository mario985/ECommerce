using ECommerce.Modules.Catalog.Application.Products.GetProductById;
using ECommerce.Modules.Catalog.Application.Products.SearchProducts;
using ECommerce.Common.Application.Errors;
using ECommerce.Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Modules.Catalog.Presentation.Products;

public static class ProductEndpoints
{
    public static RouteGroupBuilder MapProductEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", GetProductByIdAsync)
            .WithName("Catalog.GetProduct")
            .WithTags("Catalog");
        group.MapGet("/", SearchProductsAsync)
            .WithName("Catalog.SearchProducts")
            .WithTags("Catalog");
        group.MapGet("/search", AdvancedSearchProductsAsync)
            .WithName("Catalog.AdvancedSearchProducts")
            .WithTags("Catalog")
            .Produces<SearchProductsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return group;
    }

    private static async Task<IResult> GetProductByIdAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        ProductResponse? product = await sender.Send(
            new GetProductByIdQuery(id),
            cancellationToken);

        return product is null
            ? ApiResults.Problem(new ECommerce.Common.Application.Errors.Error(
                "Catalog.ProductNotFound", "The requested product was not found.", ECommerce.Common.Application.Errors.ErrorType.NotFound))
            : Results.Ok(product);
    }

    private static async Task<IResult> SearchProductsAsync(
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

    private static async Task<IResult> AdvancedSearchProductsAsync(
        ISender sender,
        CancellationToken cancellationToken,
        string? q = null,
        Guid? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        bool? inStock = null,
        double? minimumRating = null,
        string? sort = null,
        int page = 1,
        int pageSize = 20)
    {
        Result<SearchProductsResponse> result = await sender.Send(
            new SearchProductsQuery(
                q,
                categoryId,
                minPrice,
                maxPrice,
                inStock,
                minimumRating,
                sort,
                page,
                pageSize,
                IsActive: true),
            cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : ApiResults.Problem(result.Error!);
    }

}
