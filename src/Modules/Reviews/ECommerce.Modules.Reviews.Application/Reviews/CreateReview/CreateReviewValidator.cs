using ECommerce.Modules.Reviews.Domain.Reviews;
using FluentValidation;

namespace ECommerce.Modules.Reviews.Application.Reviews.CreateReview;

public sealed class CreateReviewValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.Rating).InclusiveBetween(1, 5);
        RuleFor(command => command.Comment).NotEmpty().MaximumLength(Review.MaximumCommentLength);
    }
}
