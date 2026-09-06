using ECommerce.Common.Application.Errors;
using MediatR;

namespace ECommerce.Modules.Identity.Application.Authentication.Register;

public sealed record RegisterUserCommand(string Email, string Password)
    : IRequest<Result<RegisterUserResponse>>;
