using ECommerce.Common.Application.Errors;
using MediatR;
namespace ECommerce.Modules.Catalog.Application.Categories.GetCategories;
public sealed record GetCategoriesQuery(bool ActiveOnly = true) : IRequest<Result<IReadOnlyCollection<CategoryResponse>>>;
