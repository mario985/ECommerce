using ECommerce.Common.Application.Authentication;
using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Identity.Application.Abstractions;
using ECommerce.Modules.Identity.Application.Authentication.RefreshToken;
using ECommerce.Modules.Identity.Domain.RefreshTokens;
using ECommerce.Modules.Identity.Domain.Users.Events;
using FluentValidation.Results;
using MediatR;
using RefreshTokenEntity = ECommerce.Modules.Identity.Domain.RefreshTokens.RefreshToken;

namespace ECommerce.Modules.Identity.Application.Authentication.Logout;

public sealed class LogoutCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    ICurrentUser currentUser,
    LogoutValidator validator,
    RefreshTokenRevokedDomainEventHandler revokedEventHandler,
    TimeProvider timeProvider)
    : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid || !currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            return InvalidToken();
        }

        RefreshTokenEntity? refreshToken = await refreshTokenRepository.GetByTokenAsync(
            request.RefreshToken,
            cancellationToken);
        if (refreshToken is null || refreshToken.UserId != currentUser.UserId.Value)
        {
            return Result.Failure(new Error(
                RefreshTokenErrors.NotOwnedCode,
                RefreshTokenErrors.NotOwnedDescription,
                ErrorType.Unauthorized));
        }

        if (!refreshToken.Revoke(timeProvider.GetUtcNow()))
        {
            return Result.Success();
        }

        await refreshTokenRepository.SaveChangesAsync(cancellationToken);

        RefreshTokenRevokedDomainEvent domainEvent = refreshToken.DomainEvents
            .OfType<RefreshTokenRevokedDomainEvent>()
            .Single();
        await revokedEventHandler.HandleAsync(domainEvent, cancellationToken);
        refreshToken.ClearDomainEvents();

        return Result.Success();
    }

    private static Result InvalidToken()
    {
        return Result.Failure(new Error(
            RefreshTokenErrors.InvalidCode,
            RefreshTokenErrors.InvalidDescription,
            ErrorType.Unauthorized));
    }
}
