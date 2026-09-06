using ECommerce.Common.Application.Authentication;
using ECommerce.Common.Application.Errors;
using ECommerce.Common.Presentation;
using ECommerce.Common.Presentation.Results;
using ECommerce.Modules.Catalog.Application.Categories;
using ECommerce.Modules.Catalog.Application.Categories.ActivateCategory;
using ECommerce.Modules.Catalog.Application.Categories.CreateCategory;
using ECommerce.Modules.Catalog.Application.Categories.DeactivateCategory;
using ECommerce.Modules.Catalog.Application.Categories.GetCategories;
using ECommerce.Modules.Catalog.Application.Categories.GetCategory;
using ECommerce.Modules.Catalog.Application.Categories.UpdateCategory;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Modules.Catalog.Presentation.Categories;

public static class CategoryEndpoints
{
    public static RouteGroupBuilder MapPublicCategoryEndpoints(this RouteGroupBuilder group)
    { group.MapGet("/", async (ISender s, CancellationToken ct) => ToHttp(await s.Send(new GetCategoriesQuery(), ct))).WithName("Catalog.GetCategories").WithTags("Catalog"); return group; }
    public static RouteGroupBuilder MapAdminCategoryEndpoints(this RouteGroupBuilder group)
    {
        group.RequireAuthorization(AuthorizationPolicyNames.AdminOnly);
        group.MapGet("/", async (ISender s, CancellationToken ct) => ToHttp(await s.Send(new GetCategoriesQuery(false), ct))).WithName("CatalogAdmin.GetCategories").WithTags("Catalog Admin");
        group.MapPost("/", async (CreateCategoryRequest r, ISender s, CancellationToken ct) => ToHttp(await s.Send(new CreateCategoryCommand(r.Name,r.Slug,r.Description),ct))).WithName("CatalogAdmin.CreateCategory").WithTags("Catalog Admin");
        group.MapPut("/{id:guid}", async (Guid id, UpdateCategoryRequest r, ISender s, CancellationToken ct) => ToHttp(await s.Send(new UpdateCategoryCommand(id,r.Name,r.Slug,r.Description),ct))).WithName("CatalogAdmin.UpdateCategory").WithTags("Catalog Admin");
        group.MapPost("/{id:guid}/activate", async (Guid id, ISender s, CancellationToken ct) => ToHttp(await s.Send(new ActivateCategoryCommand(id),ct))).WithName("CatalogAdmin.ActivateCategory").WithTags("Catalog Admin");
        group.MapPost("/{id:guid}/deactivate", async (Guid id, ISender s, CancellationToken ct) => ToHttp(await s.Send(new DeactivateCategoryCommand(id),ct))).WithName("CatalogAdmin.DeactivateCategory").WithTags("Catalog Admin");
        return group;
    }
    private static IResult ToHttp(Result result) => result.IsSuccess ? Results.NoContent() : ApiResults.Problem(result.Error!);
    private static IResult ToHttp(Result<Guid> result) => result.IsSuccess ? Results.Created($"/api/v1/catalog/categories/{result.Value}", new { Id = result.Value }) : ApiResults.Problem(result.Error!);
    private static IResult ToHttp(Result<IReadOnlyCollection<CategoryResponse>> result) => result.IsSuccess ? Results.Ok(result.Value) : ApiResults.Problem(result.Error!);
}
