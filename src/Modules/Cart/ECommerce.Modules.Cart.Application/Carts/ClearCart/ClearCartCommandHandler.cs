using ECommerce.Common.Application.Authentication;
using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Cart.Application.Abstractions;
using ECommerce.Modules.Cart.Domain.Carts;
using MediatR;
using Microsoft.Extensions.Logging;
using CartAggregate = ECommerce.Modules.Cart.Domain.Carts.Cart;

namespace ECommerce.Modules.Cart.Application.Carts.ClearCart;

public sealed class ClearCartCommandHandler(
    ICartRepository cartRepository,
    ICurrentUser currentUser,
    TimeProvider timeProvider,
    ILogger<ClearCartCommandHandler> logger)
    : IRequestHandler<ClearCartCommand, Result>
{
    public async Task<Result> Handle(
        ClearCartCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            return Result.Failure(new Error(
                CartErrors.UnauthorizedCode,
                CartErrors.UnauthorizedDescription,
                ErrorType.Unauthorized));
        }

        Guid customerId = currentUser.UserId.Value;
        CartAggregate? cart = await cartRepository.GetByCustomerIdAsync(
            customerId,
            cancellationToken);
        if (cart is null)
        {
            CartLog.ClearSkipped(logger, customerId);
            return Result.Success();
        }

        bool changed = cart.Clear(timeProvider.GetUtcNow());
        await cartRepository.SaveChangesAsync(cancellationToken);
        if (changed)
        {
            CartLog.Cleared(logger, cart.Id, customerId);
        }
        else
        {
            CartLog.ClearSkipped(logger, customerId);
        }

        return Result.Success();
    }
}
