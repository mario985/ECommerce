using ECommerce.Common.Application.Errors;
using MediatR;

namespace ECommerce.Modules.Reviews.Application.Reviews.GetPendingReviews;

public sealed record GetPendingReviewsQuery(int Page = 1, int PageSize = 20) : IRequest<Result<PendingReviewsResponse>>;
public sealed record PendingReviewResponse(Guid Id, Guid ProductId, int Rating, string Comment, DateTimeOffset CreatedAtUtc);
public sealed record PendingReviewsResponse(IReadOnlyCollection<PendingReviewResponse> Items, int Page, int PageSize, int TotalCount, int TotalPages);
