using FluentValidation;

namespace ECommerce.Modules.Cart.Application.Carts.RemoveItem;

public sealed class RemoveCartItemValidator : AbstractValidator<RemoveCartItemCommand>
{
    public RemoveCartItemValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
    }
}
