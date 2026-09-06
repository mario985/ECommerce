using ECommerce.Common.Application.Errors;
using MediatR;

namespace ECommerce.Modules.Identity.Application.Authentication.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken)
    : IRequest<Result<RefreshTokenResponse>>;
