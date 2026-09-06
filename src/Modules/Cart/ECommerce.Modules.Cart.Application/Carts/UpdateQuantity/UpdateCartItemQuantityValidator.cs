using FluentValidation;

namespace ECommerce.Modules.Cart.Application.Carts.UpdateQuantity;

public sealed class UpdateCartItemQuantityValidator
    : AbstractValidator<UpdateCartItemQuantityCommand>
{
    public UpdateCartItemQuantityValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.Quantity).GreaterThan(0);
    }
}
