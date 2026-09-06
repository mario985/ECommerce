namespace ECommerce.Modules.Ordering.Contracts.Reviews;

public interface IOrderReviewEligibilityReader
{
    Task<bool> HasPurchasedProductAsync(
        Guid customerId,
        Guid productId,
        CancellationToken cancellationToken);
}
