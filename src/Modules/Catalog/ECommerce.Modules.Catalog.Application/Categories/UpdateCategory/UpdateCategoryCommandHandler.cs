using ECommerce.Common.Application.Caching;
using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Catalog.Application.Caching;
using ECommerce.Modules.Catalog.Domain.Categories;
using MediatR;
namespace ECommerce.Modules.Catalog.Application.Categories.UpdateCategory;
public sealed class UpdateCategoryCommandHandler(
    ICategoryRepository repository,
    ICacheService cache,
    ICatalogCacheInvalidator? cacheInvalidator = null) : IRequestHandler<UpdateCategoryCommand, Result>
{
    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 100) return Result.Failure(CategoryErrors.InvalidName);
        if (!System.Text.RegularExpressions.Regex.IsMatch(request.Slug ?? string.Empty, "^[a-z0-9]+(?:-[a-z0-9]+)*$")) return Result.Failure(CategoryErrors.InvalidSlug);
        Category? category = await repository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null) return Result.Failure(CategoryErrors.NotFound);
        if (await repository.ExistsByNameAsync(request.Name, request.CategoryId, cancellationToken)) return Result.Failure(CategoryErrors.DuplicateName);
        if (await repository.ExistsBySlugAsync(request.Slug ?? string.Empty, request.CategoryId, cancellationToken)) return Result.Failure(CategoryErrors.DuplicateSlug);
        category.Update(request.Name, request.Slug ?? string.Empty, request.Description, DateTimeOffset.UtcNow);
        await repository.UpdateAsync(category, cancellationToken); await cache.RemoveAsync(CatalogCacheKeys.ActiveCategories, cancellationToken);
        if (cacheInvalidator is not null)
            await cacheInvalidator.InvalidateSearchesAsync(cancellationToken);
        return Result.Success();
    }
}
