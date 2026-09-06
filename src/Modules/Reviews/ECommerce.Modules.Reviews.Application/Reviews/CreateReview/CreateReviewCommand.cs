using ECommerce.Common.Application.Errors;
using MediatR;

namespace ECommerce.Modules.Reviews.Application.Reviews.CreateReview;

public sealed record CreateReviewCommand(Guid ProductId, int Rating, string Comment) : IRequest<Result<Guid>>;
