using ECommerce.Common.Application.Errors;
using MediatR;
namespace ECommerce.Modules.Catalog.Application.Categories.DeactivateCategory;
public sealed record DeactivateCategoryCommand(Guid CategoryId) : IRequest<Result>;
