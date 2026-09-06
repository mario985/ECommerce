namespace ECommerce.Modules.Reviews.Application.Caching;

public interface IReviewCacheInvalidator
{
    Task InvalidateProductAsync(Guid productId, CancellationToken cancellationToken);
}
