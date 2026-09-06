using ECommerce.Modules.Payments.Domain.Payments;

namespace ECommerce.Modules.Payments.Application.Abstractions;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid paymentId, CancellationToken cancellationToken);

    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);

    Task<Payment?> GetByProviderPaymentIntentIdAsync(
        string paymentIntentId,
        CancellationToken cancellationToken);

    Task<bool> ExistsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);

    Task AddAsync(Payment payment, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
