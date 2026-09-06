using ECommerce.Common.Application.Errors;
using MediatR;
namespace ECommerce.Modules.Catalog.Application.Categories.ActivateCategory;
public sealed record ActivateCategoryCommand(Guid CategoryId) : IRequest<Result>;
