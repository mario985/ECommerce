using ECommerce.Common.Application.Authentication;
using ECommerce.Common.Application.Errors;
using ECommerce.Common.Application.Observability;
using ECommerce.Modules.Cart.Application.Abstractions;
using ECommerce.Modules.Cart.Contracts.IntegrationEvents;
using ECommerce.Modules.Cart.Domain.Carts;
using ECommerce.Modules.Cart.Domain.Carts.Events;
using ECommerce.Modules.Cart.Domain.Checkouts;
using ECommerce.Modules.Catalog.Contracts.Products;
using MediatR;
using Microsoft.Extensions.Logging;
using CartAggregate = ECommerce.Modules.Cart.Domain.Carts.Cart;

namespace ECommerce.Modules.Cart.Application.Carts.Checkout;

public sealed class CheckoutCartCommandHandler(
    ICartRepository cartRepository,
    ICartCheckoutRepository checkoutRepository,
    IProductCatalogReader productCatalogReader,
    ICurrentUser currentUser,
    CartCheckoutRequestedDomainEventHandler domainEventHandler,
    TimeProvider timeProvider,
    ILogger<CheckoutCartCommandHandler> logger)
    : IRequestHandler<CheckoutCartCommand, Result<CheckoutResponse>>
{
    public async Task<Result<CheckoutResponse>> Handle(
        CheckoutCartCommand request,
        CancellationToken cancellationToken)
    {
        using System.Diagnostics.Activity? activity = CommerceActivitySources.Cart.StartActivity("Cart.Checkout");
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            return Failure(
                CartErrors.UnauthorizedCode,
                CartErrors.UnauthorizedDescription,
                ErrorType.Unauthorized);
        }

        Guid customerId = currentUser.UserId.Value;
        activity?.SetTag("ecommerce.customer.id", customerId);
        CartAggregate? cart = await cartRepository.GetByCustomerIdAsync(
            customerId,
            cancellationToken);
        if (cart is null)
        {
            activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, CartErrors.NotFoundCode);
            return Failure(CartErrors.NotFoundCode, CartErrors.NotFoundDescription, ErrorType.NotFound);
        }

        activity?.SetTag("ecommerce.cart.id", cart.Id);

        if (cart.Items.Count == 0)
        {
            CartLog.EmptyCheckoutRejected(logger, cart.Id, customerId);
            return Failure(CartErrors.EmptyCode, CartErrors.EmptyDescription, ErrorType.Validation);
        }


        CartCheckout? pendingCheckout = await checkoutRepository.GetPendingByCartIdAsync(
            cart.Id, cancellationToken);
        if (pendingCheckout is not null)
        {
            CartLog.CheckoutAlreadyPending(logger, pendingCheckout.CheckoutId, cart.Id, customerId);
            return Failure(
                CartCheckoutErrors.AlreadyPendingCode,
                CartCheckoutErrors.AlreadyPendingDescription,
                ErrorType.Conflict);
        }

        List<ProductCatalogResponse> products = new(cart.Items.Count);
        foreach (CartItem item in cart.Items)
        {
            ProductCatalogResponse? product = await productCatalogReader.GetByIdAsync(
                item.ProductId,
                cancellationToken);
            if (product is null)
            {
                CartLog.CheckoutProductRejected(logger, cart.Id, customerId, item.ProductId, "missing");
                return Failure(
                    CartErrors.ProductNotFoundCode,
                    CartErrors.ProductNotFoundDescription,
                    ErrorType.NotFound);
            }

            if (!product.IsActive)
            {
                CartLog.CheckoutProductRejected(logger, cart.Id, customerId, item.ProductId, "inactive");
                return Failure(
                    CartErrors.ProductInactiveCode,
                    CartErrors.ProductInactiveDescription,
                    ErrorType.Validation);
            }

            if (!string.Equals(cart.Currency, product.Currency, StringComparison.OrdinalIgnoreCase))
            {
                CartLog.CheckoutProductRejected(logger, cart.Id, customerId, item.ProductId, "currency");
                return Failure(
                    CartErrors.InvalidCheckoutCode,
                    CartErrors.InvalidCheckoutDescription,
                    ErrorType.Validation);
            }

            products.Add(product);
        }

        DateTimeOffset utcNow = timeProvider.GetUtcNow();
        foreach (ProductCatalogResponse product in products)
        {
            cart.RefreshItemSnapshot(
                product.Id,
                product.Name,
                product.Sku,
                product.Price,
                product.Currency,
                utcNow);
        }

        Guid checkoutId = Guid.NewGuid();
        activity?.SetTag("ecommerce.checkout.id", checkoutId);
        if (!cart.RequestCheckout(checkoutId))
        {
            return Failure(CartErrors.EmptyCode, CartErrors.EmptyDescription, ErrorType.Validation);
        }

        CartCheckoutRequestedDomainEvent domainEvent = cart.DomainEvents
            .OfType<CartCheckoutRequestedDomainEvent>()
            .Single(domainEvent => domainEvent.CheckoutId == checkoutId);
        ECommerce.Modules.Cart.Contracts.IntegrationEvents.CartCheckoutItem[] checkoutItems = cart.Items
            .Select(item => new ECommerce.Modules.Cart.Contracts.IntegrationEvents.CartCheckoutItem(
                item.ProductId,
                item.ProductName,
                item.Sku,
                item.UnitPrice,
                item.Currency,
                item.Quantity))
            .ToArray();

        CartCheckout checkout = CartCheckout.Create(
            checkoutId,
            cart.Id,
            customerId,
            cart.Items.Select(item => new CartCheckoutItemSnapshot(
                item.ProductId, item.ProductName, item.Sku, item.UnitPrice,
                item.Currency, item.Quantity)).ToArray(),
            utcNow);

        await checkoutRepository.AddAsync(checkout, cancellationToken);
        await checkoutRepository.SaveChangesAsync(cancellationToken);
        CartLog.CheckoutCreated(logger, checkoutId, cart.Id, customerId, checkoutItems.Length);
        await domainEventHandler.HandleAsync(domainEvent, checkoutItems, cancellationToken);
        cart.ClearDomainEvents();
        checkout.ClearDomainEvents();
        CartLog.CheckoutRequested(logger, checkoutId, cart.Id, customerId, checkoutItems.Length);

        return Result.Success(new CheckoutResponse(checkoutId));
    }

    private static Result<CheckoutResponse> Failure(
        string code,
        string description,
        ErrorType type) =>
        Result.Failure<CheckoutResponse>(new Error(code, description, type));
}
