using ECommerce.Common.Application.Caching;
using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Catalog.Application.Caching;
using ECommerce.Modules.Catalog.Domain.Categories;
using MediatR;

namespace ECommerce.Modules.Catalog.Application.Categories.CreateCategory;
public sealed class CreateCategoryCommandHandler(
    ICategoryRepository repository,
    ICacheService cache,
    ICatalogCacheInvalidator? cacheInvalidator = null) : IRequestHandler<CreateCategoryCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 100)
            return Result.Failure<Guid>(CategoryErrors.InvalidName);
        if (!System.Text.RegularExpressions.Regex.IsMatch(request.Slug ?? string.Empty, "^[a-z0-9]+(?:-[a-z0-9]+)*$"))
            return Result.Failure<Guid>(CategoryErrors.InvalidSlug);
        if (await repository.ExistsByNameAsync(request.Name, null, cancellationToken)) return Result.Failure<Guid>(CategoryErrors.DuplicateName);
        if (await repository.ExistsBySlugAsync(request.Slug ?? string.Empty, null, cancellationToken)) return Result.Failure<Guid>(CategoryErrors.DuplicateSlug);
        Category category = Category.Create(request.Name, request.Slug ?? string.Empty, request.Description, DateTimeOffset.UtcNow);
        await repository.AddAsync(category, cancellationToken);
        await cache.RemoveAsync(CatalogCacheKeys.ActiveCategories, cancellationToken);
        if (cacheInvalidator is not null)
            await cacheInvalidator.InvalidateSearchesAsync(cancellationToken);
        return Result.Success(category.Id);
    }
}
