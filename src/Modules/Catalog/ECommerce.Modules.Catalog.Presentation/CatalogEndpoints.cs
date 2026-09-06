using ECommerce.Modules.Catalog.Presentation.Products;
using ECommerce.Modules.Catalog.Presentation.Admin;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using ECommerce.Common.Presentation;
using ECommerce.Modules.Catalog.Presentation.Categories;

namespace ECommerce.Modules.Catalog.Presentation;

public static class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGroup("/api/v1/catalog/products")
            .RequireRateLimiting(ECommerce.Common.Presentation.RateLimitingPolicyNames.PublicRead)
            .MapProductEndpoints();
        endpoints
            .MapGroup("/api/v1/admin/catalog/products")
            .RequireRateLimiting(ECommerce.Common.Presentation.RateLimitingPolicyNames.Admin)
            .MapAdminProductEndpoints();
        endpoints.MapGroup("/api/v1/catalog/categories")
            .RequireRateLimiting(ECommerce.Common.Presentation.RateLimitingPolicyNames.PublicRead)
            .MapPublicCategoryEndpoints();
        endpoints.MapGroup("/api/v1/admin/catalog/categories")
            .RequireRateLimiting(ECommerce.Common.Presentation.RateLimitingPolicyNames.Admin)
            .MapAdminCategoryEndpoints();

        return endpoints;
    }
}
