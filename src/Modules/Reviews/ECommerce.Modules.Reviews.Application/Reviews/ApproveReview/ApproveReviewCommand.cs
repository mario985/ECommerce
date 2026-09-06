using ECommerce.Common.Application.Errors;
using MediatR;

namespace ECommerce.Modules.Reviews.Application.Reviews.ApproveReview;

public sealed record ApproveReviewCommand(Guid ReviewId) : IRequest<Result>;
