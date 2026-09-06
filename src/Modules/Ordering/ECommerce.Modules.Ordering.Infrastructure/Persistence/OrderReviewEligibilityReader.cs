using ECommerce.Modules.Ordering.Contracts.Reviews;
using ECommerce.Modules.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Ordering.Infrastructure.Persistence;

internal sealed class OrderReviewEligibilityReader(OrderingDbContext dbContext)
    : IOrderReviewEligibilityReader
{
    public Task<bool> HasPurchasedProductAsync(
        Guid customerId,
        Guid productId,
        CancellationToken cancellationToken) =>
        dbContext.Orders.AsNoTracking().AnyAsync(
            order => order.CustomerId == customerId &&
                     order.Status == OrderStatus.Paid &&
                     order.Lines.Any(line => line.ProductId == productId),
            cancellationToken);
}
