using FluentValidation;

namespace ECommerce.Modules.Ordering.Application.Shipments.UpdateShipmentStatus;

public sealed class UpdateShipmentStatusValidator : AbstractValidator<UpdateShipmentStatusCommand>
{
    public UpdateShipmentStatusValidator()
    {
        RuleFor(command => command.ShipmentId).NotEmpty();
        RuleFor(command => command.Status).IsInEnum();
    }
}
