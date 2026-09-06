using FluentValidation;

namespace ECommerce.Modules.Inventory.Application.Admin.SearchInventory;

public sealed class SearchInventoryValidator : AbstractValidator<SearchInventoryQuery>
{
    public SearchInventoryValidator()
    {
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
        RuleFor(query => query.AvailableQuantityLessThan)
            .GreaterThanOrEqualTo(0)
            .When(query => query.AvailableQuantityLessThan.HasValue);
    }
}
