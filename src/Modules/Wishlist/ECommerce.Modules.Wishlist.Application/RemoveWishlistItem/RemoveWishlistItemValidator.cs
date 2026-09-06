using FluentValidation;

namespace ECommerce.Modules.Wishlist.Application.RemoveWishlistItem;

public sealed class RemoveWishlistItemValidator : AbstractValidator<RemoveWishlistItemCommand>
{
    public RemoveWishlistItemValidator() => RuleFor(command => command.ProductId).NotEmpty();
}
