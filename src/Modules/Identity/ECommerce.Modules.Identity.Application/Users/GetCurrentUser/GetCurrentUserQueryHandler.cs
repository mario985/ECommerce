using ECommerce.Common.Application.Authentication;
using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Identity.Application.Abstractions;
using ECommerce.Modules.Identity.Domain.Users;
using MediatR;

namespace ECommerce.Modules.Identity.Application.Users.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler(ICurrentUser currentUser)
    : IRequestHandler<GetCurrentUserQuery, Result<CurrentUserResponse>>
{
    public Task<Result<CurrentUserResponse>> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated ||
            currentUser.UserId is null ||
            string.IsNullOrWhiteSpace(currentUser.Email))
        {
            return Task.FromResult(Result.Failure<CurrentUserResponse>(new Error(
                UserErrors.InvalidCredentialsCode,
                UserErrors.InvalidCredentialsDescription,
                ErrorType.Unauthorized)));
        }

        return Task.FromResult(Result.Success(new CurrentUserResponse(
            currentUser.UserId.Value,
            currentUser.Email,
            currentUser.Roles)));
    }
}
