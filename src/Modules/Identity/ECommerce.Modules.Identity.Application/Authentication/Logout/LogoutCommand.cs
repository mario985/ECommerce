using ECommerce.Common.Application.Errors;
using MediatR;

namespace ECommerce.Modules.Identity.Application.Authentication.Logout;

public sealed record LogoutCommand(string RefreshToken) : IRequest<Result>;
