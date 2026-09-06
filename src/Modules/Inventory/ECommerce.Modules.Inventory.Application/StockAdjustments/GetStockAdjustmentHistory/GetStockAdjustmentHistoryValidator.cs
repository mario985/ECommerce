using FluentValidation;

namespace ECommerce.Modules.Inventory.Application.StockAdjustments.GetStockAdjustmentHistory;

public sealed class GetStockAdjustmentHistoryValidator
    : AbstractValidator<GetStockAdjustmentHistoryQuery>
{
    public GetStockAdjustmentHistoryValidator()
    {
        RuleFor(query => query.ProductId).NotEmpty();
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
        RuleFor(query => query.Type).IsInEnum().When(query => query.Type.HasValue);
        RuleFor(query => query.Reason).IsInEnum().When(query => query.Reason.HasValue);
        RuleFor(query => query.ToUtc)
            .GreaterThanOrEqualTo(query => query.FromUtc)
            .When(query => query.FromUtc.HasValue && query.ToUtc.HasValue);
    }
}
