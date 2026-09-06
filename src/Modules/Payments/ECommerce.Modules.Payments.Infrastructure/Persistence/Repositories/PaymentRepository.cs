using ECommerce.Modules.Payments.Application.Abstractions;
using ECommerce.Modules.Payments.Domain.Payments;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Payments.Infrastructure.Persistence.Repositories;

internal sealed class PaymentRepository(PaymentsDbContext dbContext) : IPaymentRepository
{
    public Task<Payment?> GetByIdAsync(Guid paymentId, CancellationToken cancellationToken) =>
        dbContext.Payments
            .Include(payment => payment.Attempts)
            .SingleOrDefaultAsync(payment => payment.Id == paymentId, cancellationToken);

    public Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken) =>
        dbContext.Payments
            .Include(payment => payment.Attempts)
            .SingleOrDefaultAsync(payment => payment.OrderId == orderId, cancellationToken);

    public Task<Payment?> GetByProviderPaymentIntentIdAsync(
        string paymentIntentId,
        CancellationToken cancellationToken) =>
        dbContext.Payments
            .Include(payment => payment.Attempts)
            .SingleOrDefaultAsync(
                payment => payment.ProviderPaymentIntentId == paymentIntentId,
                cancellationToken);

    public Task<bool> ExistsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken) =>
        dbContext.Payments.AnyAsync(payment => payment.OrderId == orderId, cancellationToken);

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken)
    {
        await dbContext.Payments.AddAsync(payment, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
