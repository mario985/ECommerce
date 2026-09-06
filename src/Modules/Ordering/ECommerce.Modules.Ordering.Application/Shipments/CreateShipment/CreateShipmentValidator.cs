using ECommerce.Modules.Ordering.Domain.Shipments;
using FluentValidation;

namespace ECommerce.Modules.Ordering.Application.Shipments.CreateShipment;

public sealed class CreateShipmentValidator : AbstractValidator<CreateShipmentCommand>
{
    public CreateShipmentValidator()
    {
        RuleFor(command => command.OrderId).NotEmpty();
        RuleFor(command => command.Carrier)
            .NotEmpty()
            .MaximumLength(Shipment.MaximumCarrierLength);
        RuleFor(command => command.TrackingNumber)
            .NotEmpty()
            .MaximumLength(Shipment.MaximumTrackingNumberLength);
    }
}
