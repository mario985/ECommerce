using ECommerce.Common.Application.Errors;
using MediatR;
namespace ECommerce.Modules.Catalog.Application.Categories.GetCategory;
public sealed record GetCategoryQuery(Guid CategoryId) : IRequest<Result<CategoryResponse>>;
