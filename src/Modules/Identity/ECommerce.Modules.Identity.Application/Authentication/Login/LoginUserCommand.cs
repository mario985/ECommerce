using ECommerce.Common.Application.Errors;
using MediatR;

namespace ECommerce.Modules.Identity.Application.Authentication.Login;

public sealed record LoginUserCommand(string Email, string Password)
    : IRequest<Result<AuthenticationResponse>>;
