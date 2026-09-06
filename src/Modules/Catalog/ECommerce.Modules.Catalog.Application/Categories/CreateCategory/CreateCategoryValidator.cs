using FluentValidation;

namespace ECommerce.Modules.Catalog.Application.Categories.CreateCategory;
public sealed class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Slug).Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").WithMessage("Slug must be URL-friendly.");
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
