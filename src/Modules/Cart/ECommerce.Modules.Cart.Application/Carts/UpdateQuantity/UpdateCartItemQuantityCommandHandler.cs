using ECommerce.Common.Application.Authentication;
using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Cart.Application.Abstractions;
using ECommerce.Modules.Cart.Application.Carts.GetCart;
using ECommerce.Modules.Cart.Domain.Carts;
using FluentValidation.Results;
using MediatR;
using CartAggregate = ECommerce.Modules.Cart.Domain.Carts.Cart;

namespace ECommerce.Modules.Cart.Application.Carts.UpdateQuantity;

public sealed class UpdateCartItemQuantityCommandHandler(
    ICartRepository cartRepository,
    ICurrentUser currentUser,
    UpdateCartItemQuantityValidator validator,
    TimeProvider timeProvider)
    : IRequestHandler<UpdateCartItemQuantityCommand, Result<CartResponse>>
{
    public async Task<Result<CartResponse>> Handle(
        UpdateCartItemQuantityCommand request,
        CancellationToken cancellationToken)
    {
        ValidationResult validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            bool invalidProductId = request.ProductId == Guid.Empty;
            return Failure(
                invalidProductId ? CartErrors.InvalidProductIdCode : CartErrors.InvalidQuantityCode,
                invalidProductId
                    ? CartErrors.InvalidProductIdDescription
                    : CartErrors.InvalidQuantityDescription,
                ErrorType.Validation);
        }

        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            return Failure(
                CartErrors.UnauthorizedCode,
                CartErrors.UnauthorizedDescription,
                ErrorType.Unauthorized);
        }

        CartAggregate? cart = await cartRepository.GetByCustomerIdAsync(
            currentUser.UserId.Value,
            cancellationToken);
        if (cart is null)
        {
            return Failure(
                CartErrors.NotFoundCode,
                CartErrors.NotFoundDescription,
                ErrorType.NotFound);
        }

        if (cart.FindItem(request.ProductId) is null)
        {
            return Failure(
                CartErrors.ItemNotFoundCode,
                CartErrors.ItemNotFoundDescription,
                ErrorType.NotFound);
        }

        cart.UpdateItemQuantity(
            request.ProductId,
            request.Quantity,
            timeProvider.GetUtcNow());
        await cartRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(CartResponse.From(cart));
    }

    private static Result<CartResponse> Failure(
        string code,
        string description,
        ErrorType type) =>
        Result.Failure<CartResponse>(new Error(code, description, type));
}
