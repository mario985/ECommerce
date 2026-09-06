using FluentValidation;

namespace ECommerce.Modules.Reviews.Application.Reviews.GetPendingReviews;

public sealed class GetPendingReviewsValidator : AbstractValidator<GetPendingReviewsQuery>
{
    public GetPendingReviewsValidator()
    {
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}
