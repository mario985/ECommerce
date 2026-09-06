using FluentValidation;

namespace ECommerce.Modules.Ordering.Application.Orders.GetOrderHistory;

public sealed class GetOrderHistoryValidator : AbstractValidator<GetOrderHistoryQuery>
{
    public GetOrderHistoryValidator()
    {
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
        RuleFor(query => query.Status)
            .IsInEnum()
            .When(query => query.Status.HasValue);
    }
}
