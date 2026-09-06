using FluentValidation;

namespace ECommerce.Modules.Identity.Application.Authentication.Logout;

public sealed class LogoutValidator : AbstractValidator<LogoutCommand>
{
    public LogoutValidator()
    {
        RuleFor(command => command.RefreshToken)
            .NotEmpty()
            .MaximumLength(512);
    }
}
