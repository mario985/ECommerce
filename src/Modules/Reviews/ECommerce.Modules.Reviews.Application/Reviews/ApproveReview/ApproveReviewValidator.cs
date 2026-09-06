using FluentValidation;

namespace ECommerce.Modules.Reviews.Application.Reviews.ApproveReview;

public sealed class ApproveReviewValidator : AbstractValidator<ApproveReviewCommand>
{
    public ApproveReviewValidator() => RuleFor(command => command.ReviewId).NotEmpty();
}
