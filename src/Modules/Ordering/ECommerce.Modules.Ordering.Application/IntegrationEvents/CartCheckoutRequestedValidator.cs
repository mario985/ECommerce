using ECommerce.Modules.Cart.Contracts.IntegrationEvents;
using FluentValidation;

namespace ECommerce.Modules.Ordering.Application.IntegrationEvents;

public sealed class CartCheckoutRequestedValidator
    : AbstractValidator<CartCheckoutRequestedIntegrationEvent>
{
    public CartCheckoutRequestedValidator()
    {
        RuleFor(integrationEvent => integrationEvent.CheckoutId).NotEmpty();
        RuleFor(integrationEvent => integrationEvent.CustomerId).NotEmpty();
        RuleFor(integrationEvent => integrationEvent.Items).NotEmpty();
        RuleForEach(integrationEvent => integrationEvent.Items).ChildRules(item =>
        {
            item.RuleFor(candidate => candidate.ProductId).NotEmpty();
            item.RuleFor(candidate => candidate.ProductName).NotEmpty();
            item.RuleFor(candidate => candidate.Sku).NotEmpty();
            item.RuleFor(candidate => candidate.UnitPrice).GreaterThanOrEqualTo(0);
            item.RuleFor(candidate => candidate.Currency).NotEmpty();
            item.RuleFor(candidate => candidate.Quantity).GreaterThan(0);
        });
        RuleFor(integrationEvent => integrationEvent.Items)
            .Must(items => items.Select(item => item.ProductId).Distinct().Count() == items.Count)
            .WithMessage("Checkout products must be unique.");
        RuleFor(integrationEvent => integrationEvent.Items)
            .Must(items => items.Select(item => item.Currency.Trim().ToUpperInvariant()).Distinct().Count() == 1)
            .When(integrationEvent => integrationEvent.Items.Count > 0 &&
                integrationEvent.Items.All(item => !string.IsNullOrWhiteSpace(item.Currency)))
            .WithMessage("Checkout currencies must match.");
    }
}
