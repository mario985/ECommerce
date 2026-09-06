using ECommerce.Common.Application.Errors;
using MediatR;

namespace ECommerce.Modules.Reviews.Application.Reviews.RejectReview;

public sealed record RejectReviewCommand(Guid ReviewId) : IRequest<Result>;
