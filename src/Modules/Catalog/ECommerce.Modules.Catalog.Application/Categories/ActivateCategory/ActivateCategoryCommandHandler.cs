using ECommerce.Common.Application.Caching;
using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Catalog.Application.Caching;
using ECommerce.Modules.Catalog.Domain.Categories;
using MediatR;
namespace ECommerce.Modules.Catalog.Application.Categories.ActivateCategory;
public sealed class ActivateCategoryCommandHandler(ICategoryRepository repository, ICacheService cache, ICatalogCacheInvalidator? cacheInvalidator = null) : IRequestHandler<ActivateCategoryCommand, Result>
{
 public async Task<Result> Handle(ActivateCategoryCommand request, CancellationToken cancellationToken) { Category? c=await repository.GetByIdAsync(request.CategoryId,cancellationToken); if(c is null)return Result.Failure(CategoryErrors.NotFound); if(!c.Activate(DateTimeOffset.UtcNow))return Result.Failure(CategoryErrors.AlreadyActive); await repository.UpdateAsync(c,cancellationToken); await cache.RemoveAsync(CatalogCacheKeys.ActiveCategories,cancellationToken); if(cacheInvalidator is not null) await cacheInvalidator.InvalidateSearchesAsync(cancellationToken); return Result.Success(); }
}
