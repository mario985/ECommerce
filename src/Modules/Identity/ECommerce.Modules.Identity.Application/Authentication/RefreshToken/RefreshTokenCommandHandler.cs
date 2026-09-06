using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Identity.Application.Abstractions;
using ECommerce.Modules.Identity.Domain.RefreshTokens;
using ECommerce.Modules.Identity.Domain.Users.Events;
using FluentValidation.Results;
using MediatR;
using RefreshTokenEntity = ECommerce.Modules.Identity.Domain.RefreshTokens.RefreshToken;

namespace ECommerce.Modules.Identity.Application.Authentication.RefreshToken;

public sealed class RefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IIdentityService identityService,
    ITokenService tokenService,
    RefreshTokenValidator validator,
    RefreshTokenIssuedDomainEventHandler issuedEventHandler,
    RefreshTokenRevokedDomainEventHandler revokedEventHandler,
    TimeProvider timeProvider)
    : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    public async Task<Result<RefreshTokenResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Unauthorized(RefreshTokenErrors.InvalidCode, RefreshTokenErrors.InvalidDescription);
        }

        RefreshTokenEntity? currentToken = await refreshTokenRepository.GetByTokenAsync(
            request.RefreshToken,
            cancellationToken);
        if (currentToken is null)
        {
            return Unauthorized(RefreshTokenErrors.InvalidCode, RefreshTokenErrors.InvalidDescription);
        }

        DateTimeOffset utcNow = timeProvider.GetUtcNow();
        RefreshTokenStatus status = currentToken.GetStatus(utcNow);
        if (status == RefreshTokenStatus.Expired)
        {
            return Unauthorized(RefreshTokenErrors.ExpiredCode, RefreshTokenErrors.ExpiredDescription);
        }

        if (status == RefreshTokenStatus.Revoked)
        {
            return Unauthorized(RefreshTokenErrors.RevokedCode, RefreshTokenErrors.RevokedDescription);
        }

        Result<AuthenticatedUser> userResult = await identityService.GetUserAsync(
            currentToken.UserId,
            cancellationToken);
        if (userResult.IsFailure)
        {
            return Unauthorized(RefreshTokenErrors.InvalidCode, RefreshTokenErrors.InvalidDescription);
        }

        AuthenticatedUser user = userResult.Value;
        IReadOnlyCollection<string> roles = await identityService.GetRolesAsync(
            user.UserId,
            cancellationToken);
        AccessToken accessToken = tokenService.GenerateAccessToken(user.UserId, user.Email, roles);
        RefreshTokenValue replacementValue = tokenService.GenerateRefreshToken();
        RefreshTokenEntity replacementToken = RefreshTokenEntity.Issue(
            user.UserId,
            replacementValue.Value,
            replacementValue.ExpiresAtUtc,
            utcNow);

        currentToken.Revoke(utcNow, replacementToken.Token);
        await refreshTokenRepository.AddAsync(replacementToken, cancellationToken);
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);

        await PublishEventsAsync(currentToken, replacementToken, cancellationToken);

        return Result.Success(new RefreshTokenResponse(
            accessToken.Value,
            replacementToken.Token,
            accessToken.ExpiresAtUtc,
            user.UserId,
            user.Email,
            roles));
    }

    private async Task PublishEventsAsync(
        RefreshTokenEntity revokedToken,
        RefreshTokenEntity issuedToken,
        CancellationToken cancellationToken)
    {
        RefreshTokenRevokedDomainEvent revokedEvent = revokedToken.DomainEvents
            .OfType<RefreshTokenRevokedDomainEvent>()
            .Single();
        await revokedEventHandler.HandleAsync(revokedEvent, cancellationToken);
        revokedToken.ClearDomainEvents();

        RefreshTokenIssuedDomainEvent issuedEvent = issuedToken.DomainEvents
            .OfType<RefreshTokenIssuedDomainEvent>()
            .Single();
        await issuedEventHandler.HandleAsync(issuedEvent, cancellationToken);
        issuedToken.ClearDomainEvents();
    }

    private static Result<RefreshTokenResponse> Unauthorized(string code, string description)
    {
        return Result.Failure<RefreshTokenResponse>(new Error(
            code,
            description,
            ErrorType.Unauthorized));
    }
}
