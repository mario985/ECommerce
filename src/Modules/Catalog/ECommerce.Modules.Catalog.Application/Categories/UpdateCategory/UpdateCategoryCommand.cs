using ECommerce.Common.Application.Errors;
using MediatR;
namespace ECommerce.Modules.Catalog.Application.Categories.UpdateCategory;
public sealed record UpdateCategoryCommand(Guid CategoryId, string Name, string Slug, string? Description) : IRequest<Result>;
