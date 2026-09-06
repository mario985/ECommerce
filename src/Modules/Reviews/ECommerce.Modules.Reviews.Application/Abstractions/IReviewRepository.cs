using ECommerce.Modules.Reviews.Domain.Reviews;

namespace ECommerce.Modules.Reviews.Application.Abstractions;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsByCustomerAndProductAsync(Guid customerId, Guid productId, CancellationToken cancellationToken);
    Task<(IReadOnlyCollection<Review> Reviews, int TotalCount, double AverageRating)> GetApprovedByProductAsync(Guid productId, int page, int pageSize, CancellationToken cancellationToken);
    Task<(int ReviewCount, double AverageRating)> GetRatingSummaryAsync(
        Guid productId,
        CancellationToken cancellationToken) => Task.FromResult((0, 0d));
    Task<(IReadOnlyCollection<Review> Reviews, int TotalCount)> GetPendingAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task AddAsync(Review review, CancellationToken cancellationToken);
    Task UpdateAsync(Review review, CancellationToken cancellationToken);
}
