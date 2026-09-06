using FluentValidation;

namespace ECommerce.Modules.Ordering.Application.Shipments.GetShipments;

public sealed class GetShipmentsValidator : AbstractValidator<GetShipmentsQuery>
{
    public GetShipmentsValidator()
    {
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
        RuleFor(query => query.Status).IsInEnum().When(query => query.Status.HasValue);
    }
}
