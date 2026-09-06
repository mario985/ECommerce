using ECommerce.Common.Application.Errors;
using MediatR;

namespace ECommerce.Modules.Identity.Application.Users.GetCurrentUser;

public sealed record GetCurrentUserQuery : IRequest<Result<CurrentUserResponse>>;
