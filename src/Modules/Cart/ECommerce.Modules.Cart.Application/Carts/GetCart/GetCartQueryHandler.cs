using ECommerce.Common.Application.Authentication;
using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Cart.Application.Abstractions;
using ECommerce.Modules.Cart.Domain.Carts;
using MediatR;
using CartAggregate = ECommerce.Modules.Cart.Domain.Carts.Cart;

namespace ECommerce.Modules.Cart.Application.Carts.GetCart;

public sealed class GetCartQueryHandler(
    ICartRepository cartRepository,
    ICurrentUser currentUser) : IRequestHandler<GetCartQuery, Result<CartResponse>>
{
    public async Task<Result<CartResponse>> Handle(
        GetCartQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            return Result.Failure<CartResponse>(new Error(
                CartErrors.UnauthorizedCode,
                CartErrors.UnauthorizedDescription,
                ErrorType.Unauthorized));
        }

        CartAggregate? cart = await cartRepository.GetByCustomerIdAsync(
            currentUser.UserId.Value,
            cancellationToken);

        return Result.Success(cart is null ? CartResponse.Empty() : CartResponse.From(cart));
    }
}
