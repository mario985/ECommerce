using FluentValidation;

namespace ECommerce.Modules.Identity.Application.Authentication.Login;

public sealed class LoginUserValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(command => command.Password).NotEmpty();
    }
}
