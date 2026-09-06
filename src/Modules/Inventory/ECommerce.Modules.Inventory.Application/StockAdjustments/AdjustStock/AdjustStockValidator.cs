using FluentValidation;

namespace ECommerce.Modules.Inventory.Application.StockAdjustments.AdjustStock;

public sealed class AdjustStockValidator : AbstractValidator<AdjustStockCommand>
{
    public const int MaximumNoteLength = 500;

    public AdjustStockValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.Quantity).GreaterThan(0);
        RuleFor(command => command.Type).IsInEnum();
        RuleFor(command => command.Reason).IsInEnum();
        RuleFor(command => command.Note).MaximumLength(MaximumNoteLength);
    }
}
