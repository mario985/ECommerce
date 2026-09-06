using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Cart.Application.Carts.AddItem;
using ECommerce.Modules.Cart.Contracts;
using MediatR;

namespace ECommerce.Modules.Cart.Application;

public sealed class CartItemAdder(ISender sender) : ICartItemAdder
{
    public async Task<Result> AddItemAsync(Guid productId, int quantity, CancellationToken cancellationToken)
    {
        Result result = await sender.Send(new AddCartItemCommand(productId, quantity), cancellationToken);
        return result;
    }
}
