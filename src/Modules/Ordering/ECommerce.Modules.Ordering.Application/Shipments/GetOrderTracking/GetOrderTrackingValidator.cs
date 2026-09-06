using FluentValidation;

namespace ECommerce.Modules.Ordering.Application.Shipments.GetOrderTracking;

public sealed class GetOrderTrackingValidator : AbstractValidator<GetOrderTrackingQuery>
{
    public GetOrderTrackingValidator()
    {
        RuleFor(query => query.OrderId).NotEmpty();
    }
}
