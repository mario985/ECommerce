using ECommerce.Common.Application.Errors;

namespace ECommerce.Modules.Cart.Contracts;

public interface ICartItemAdder
{
    Task<Result> AddItemAsync(Guid productId, int quantity, CancellationToken cancellationToken);
}
