using FluentValidation;

namespace ECommerce.Modules.Wishlist.Application.AddWishlistItem;

public sealed class AddWishlistItemValidator : AbstractValidator<AddWishlistItemCommand>
{
    public AddWishlistItemValidator() => RuleFor(command => command.ProductId).NotEmpty();
}
