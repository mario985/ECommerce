using ECommerce.Common.Application.Authentication;
using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Cart.Application.Abstractions;
using ECommerce.Modules.Cart.Domain.Carts;
using MediatR;
using Microsoft.Extensions.Logging;
using CartAggregate = ECommerce.Modules.Cart.Domain.Carts.Cart;

namespace ECommerce.Modules.Cart.Application.Carts.RemoveItem;

public sealed class RemoveCartItemCommandHandler(
    ICartRepository cartRepository,
    ICurrentUser currentUser,
    RemoveCartItemValidator validator,
    TimeProvider timeProvider,
    ILogger<RemoveCartItemCommandHandler> logger)
    : IRequestHandler<RemoveCartItemCommand, Result>
{
    public async Task<Result> Handle(
        RemoveCartItemCommand request,
        CancellationToken cancellationToken)
    {
        if (!(await validator.ValidateAsync(request, cancellationToken)).IsValid)
        {
            return Failure(
                CartErrors.InvalidProductIdCode,
                CartErrors.InvalidProductIdDescription,
                ErrorType.Validation);
        }

        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            return Failure(
                CartErrors.UnauthorizedCode,
                CartErrors.UnauthorizedDescription,
                ErrorType.Unauthorized);
        }

        Guid customerId = currentUser.UserId.Value;
        CartAggregate? cart = await cartRepository.GetByCustomerIdAsync(
            customerId,
            cancellationToken);
        if (cart is null)
        {
            CartLog.ItemRemovalRejected(logger, customerId, request.ProductId);
            return Failure(
                CartErrors.NotFoundCode,
                CartErrors.NotFoundDescription,
                ErrorType.NotFound);
        }

        if (!cart.RemoveItem(request.ProductId, timeProvider.GetUtcNow()))
        {
            CartLog.ItemRemovalRejected(logger, customerId, request.ProductId);
            return Failure(
                CartErrors.ItemNotFoundCode,
                CartErrors.ItemNotFoundDescription,
                ErrorType.NotFound);
        }

        await cartRepository.SaveChangesAsync(cancellationToken);
        CartLog.ItemRemoved(logger, cart.Id, customerId, request.ProductId);
        return Result.Success();
    }

    private static Result Failure(string code, string description, ErrorType type) =>
        Result.Failure(new Error(code, description, type));
}
