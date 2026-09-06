using FluentValidation;

namespace ECommerce.Modules.Catalog.Application.Products.SearchProducts;

public sealed class SearchProductsValidator : AbstractValidator<SearchProductsQuery>
{
    private static readonly string[] SupportedSorts =
        ["price-asc", "price-desc", "newest", "rating-desc", "popularity"];

    public SearchProductsValidator()
    {
        RuleFor(query => query.MinPrice)
            .GreaterThanOrEqualTo(0)
            .When(query => query.MinPrice.HasValue);
        RuleFor(query => query.MaxPrice)
            .GreaterThanOrEqualTo(0)
            .When(query => query.MaxPrice.HasValue);
        RuleFor(query => query)
            .Must(query => !query.MinPrice.HasValue || !query.MaxPrice.HasValue ||
                           query.MinPrice.Value <= query.MaxPrice.Value)
            .WithName(nameof(SearchProductsQuery.MaxPrice))
            .WithMessage("MaxPrice must be greater than or equal to MinPrice.");
        RuleFor(query => query.Page).GreaterThan(0);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
        RuleFor(query => query.MinimumRating)
            .InclusiveBetween(1, 5)
            .When(query => query.MinimumRating.HasValue);
        RuleFor(query => query.Sort)
            .Must((query, sort) => string.IsNullOrWhiteSpace(sort) ||
                          SupportedSorts.Contains(sort.Trim(), StringComparer.OrdinalIgnoreCase) ||
                          !query.IncludeSkuAndCategoryInTextSearch &&
                          string.Equals(sort.Trim(), "name", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Sort must be one of: price-asc, price-desc, newest, rating-desc, popularity.");
    }
}
