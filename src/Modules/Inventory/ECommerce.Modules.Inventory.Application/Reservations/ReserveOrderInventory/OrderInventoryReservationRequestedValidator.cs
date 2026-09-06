using ECommerce.Modules.Ordering.Contracts.IntegrationEvents;
using FluentValidation;

namespace ECommerce.Modules.Inventory.Application.Reservations.ReserveOrderInventory;

public sealed class OrderInventoryReservationRequestedValidator
    : AbstractValidator<OrderInventoryReservationRequestedIntegrationEvent>
{
    public OrderInventoryReservationRequestedValidator()
    {
        RuleFor(integrationEvent => integrationEvent.OrderId).NotEmpty();
        RuleFor(integrationEvent => integrationEvent.CheckoutId).NotEmpty();
        RuleFor(integrationEvent => integrationEvent.Items).NotEmpty();
        RuleForEach(integrationEvent => integrationEvent.Items).ChildRules(item =>
        {
            item.RuleFor(candidate => candidate.ProductId).NotEmpty();
            item.RuleFor(candidate => candidate.Quantity).GreaterThan(0);
        });
        RuleFor(integrationEvent => integrationEvent.Items)
            .Must(items => items.Select(item => item.ProductId).Distinct().Count() == items.Count)
            .WithMessage("Reservation products must be unique.");
    }
}
