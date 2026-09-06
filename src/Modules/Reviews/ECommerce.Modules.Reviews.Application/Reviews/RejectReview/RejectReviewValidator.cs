using FluentValidation;

namespace ECommerce.Modules.Reviews.Application.Reviews.RejectReview;

public sealed class RejectReviewValidator : AbstractValidator<RejectReviewCommand>
{
    public RejectReviewValidator() => RuleFor(command => command.ReviewId).NotEmpty();
}
