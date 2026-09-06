using FluentValidation;

namespace ECommerce.Modules.Reviews.Application.Reviews.GetProductReviews;

public sealed class GetProductReviewsValidator : AbstractValidator<GetProductReviewsQuery>
{
    public GetProductReviewsValidator()
    {
        RuleFor(query => query.ProductId).NotEmpty();
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}
