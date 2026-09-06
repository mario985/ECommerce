using FluentValidation;
namespace ECommerce.Modules.Catalog.Application.Categories.UpdateCategory;
public sealed class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryValidator() { RuleFor(x => x.CategoryId).NotEmpty(); RuleFor(x => x.Name).NotEmpty().MaximumLength(100); RuleFor(x => x.Slug).Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$"); RuleFor(x => x.Description).MaximumLength(500); }
}
