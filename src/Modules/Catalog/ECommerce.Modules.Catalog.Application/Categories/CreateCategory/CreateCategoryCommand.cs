using ECommerce.Common.Application.Errors;
using MediatR;

namespace ECommerce.Modules.Catalog.Application.Categories.CreateCategory;
public sealed record CreateCategoryCommand(string Name, string Slug, string? Description) : IRequest<Result<Guid>>;
