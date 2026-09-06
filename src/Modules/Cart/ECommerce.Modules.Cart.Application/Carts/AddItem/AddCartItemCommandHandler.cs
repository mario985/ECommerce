using ECommerce.Common.Application.Authentication;
using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Cart.Application.Abstractions;
using ECommerce.Modules.Cart.Application.Carts.GetCart;
using ECommerce.Modules.Cart.Domain.Carts;
using ECommerce.Modules.Catalog.Contracts.Products;
using FluentValidation.Results;
using MediatR;
using CartAggregate = ECommerce.Modules.Cart.Domain.Carts.Cart;

namespace ECommerce.Modules.Cart.Application.Carts.AddItem;

public sealed class AddCartItemCommandHandler(
    ICartRepository cartRepository,
    IProductCatalogReader productCatalogReader,
    ICurrentUser currentUser,
    AddCartItemValidator validator,
    TimeProvider timeProvider)
    : IRequestHandler<AddCartItemCommand, Result<CartResponse>>
{
    public async Task<Result<CartResponse>> Handle(
        AddCartItemCommand request,
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

        ProductCatalogResponse? product = await productCatalogReader.GetByIdAsync(
            request.ProductId,
            cancellationToken);
        if (product is null)
        {
            return Failure(
                CartErrors.ProductNotFoundCode,
                CartErrors.ProductNotFoundDescription,
                ErrorType.NotFound);
        }

        if (!product.IsActive)
        {
            return Failure(
                CartErrors.ProductInactiveCode,
                CartErrors.ProductInactiveDescription,
                ErrorType.Validation);
        }

        Guid customerId = currentUser.UserId.Value;
        CartAggregate? cart = await cartRepository.GetByCustomerIdAsync(
            customerId,
            cancellationToken);
        if (cart?.Currency is not null &&
            !string.Equals(cart.Currency, product.Currency, StringComparison.OrdinalIgnoreCase))
        {
            return Failure(
                CartErrors.CurrencyMismatchCode,
                CartErrors.CurrencyMismatchDescription,
                ErrorType.Conflict);
        }

        DateTimeOffset utcNow = timeProvider.GetUtcNow();
        if (cart is null)
        {
            cart = CartAggregate.Create(customerId, utcNow);
            await cartRepository.AddAsync(cart, cancellationToken);
        }

        cart.AddItem(
            product.Id,
            product.Name,
            product.Sku,
            product.Price,
            product.Currency,
            request.Quantity,
            utcNow);
        await cartRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(CartResponse.From(cart));
    }

    private static Result<CartResponse> Failure(
        string code,
        string description,
        ErrorType type) =>
        Result.Failure<CartResponse>(new Error(code, description, type));
}
