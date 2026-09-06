using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Catalog.Domain.Categories;
using MediatR;
namespace ECommerce.Modules.Catalog.Application.Categories.GetCategory;
public sealed class GetCategoryQueryHandler(ICategoryRepository repository) : IRequestHandler<GetCategoryQuery, Result<CategoryResponse>>
{
    public async Task<Result<CategoryResponse>> Handle(GetCategoryQuery request, CancellationToken cancellationToken)
    { Category? category = await repository.GetByIdAsync(request.CategoryId, cancellationToken); return category is null ? Result.Failure<CategoryResponse>(CategoryErrors.NotFound) : Result.Success(CategoryResponse.From(category)); }
}
