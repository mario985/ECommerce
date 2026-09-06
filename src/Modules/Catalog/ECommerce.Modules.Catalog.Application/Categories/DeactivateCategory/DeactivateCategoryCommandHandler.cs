using ECommerce.Common.Application.Caching;
using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Catalog.Application.Caching;
using ECommerce.Modules.Catalog.Domain.Categories;
using ECommerce.Modules.Catalog.Domain.Products;
using MediatR;
namespace ECommerce.Modules.Catalog.Application.Categories.DeactivateCategory;
public sealed class DeactivateCategoryCommandHandler(ICategoryRepository repository, ICacheService cache, IProductRepository? productRepository = null, ICatalogCacheInvalidator? cacheInvalidator = null) : IRequestHandler<DeactivateCategoryCommand, Result>
{
 public async Task<Result> Handle(DeactivateCategoryCommand request, CancellationToken cancellationToken) { Category? c=await repository.GetByIdAsync(request.CategoryId,cancellationToken); if(c is null)return Result.Failure(CategoryErrors.NotFound); if(productRepository is not null && await productRepository.ExistsByCategoryIdAsync(request.CategoryId, true, cancellationToken))return Result.Failure(CategoryErrors.HasActiveProducts); if(!c.Deactivate(DateTimeOffset.UtcNow))return Result.Failure(CategoryErrors.AlreadyInactive); await repository.UpdateAsync(c,cancellationToken); await cache.RemoveAsync(CatalogCacheKeys.ActiveCategories,cancellationToken); if(cacheInvalidator is not null) await cacheInvalidator.InvalidateSearchesAsync(cancellationToken); return Result.Success(); }
}
