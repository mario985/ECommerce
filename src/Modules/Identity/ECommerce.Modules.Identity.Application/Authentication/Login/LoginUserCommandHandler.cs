using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Identity.Application.Abstractions;
using ECommerce.Modules.Identity.Application.Authentication.RefreshToken;
using ECommerce.Modules.Identity.Domain.RefreshTokens;
using ECommerce.Modules.Identity.Domain.Users;
using ECommerce.Modules.Identity.Domain.Users.Events;
using FluentValidation.Results;
using MediatR;
using RefreshTokenEntity = ECommerce.Modules.Identity.Domain.RefreshTokens.RefreshToken;

namespace ECommerce.Modules.Identity.Application.Authentication.Login;

public sealed class LoginUserCommandHandler(
    IIdentityService identityService,
    ITokenService tokenService,
    IRefreshTokenRepository refreshTokenRepository,
    LoginUserValidator validator,
    UserLoggedInDomainEventHandler domainEventHandler,
    RefreshTokenIssuedDomainEventHandler refreshTokenIssuedEventHandler,
    TimeProvider timeProvider)
    : IRequestHandler<LoginUserCommand, Result<AuthenticationResponse>>
{
    public async Task<Result<AuthenticationResponse>> Handle(
        LoginUserCommand request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<AuthenticationResponse>(new Error(
                "Identity.LoginValidation",
                string.Join(" ", validationResult.Errors.Select(error => error.ErrorMessage)),
                ErrorType.Validation));
        }

        Result<AuthenticatedUser> credentialsResult = await identityService.ValidateCredentialsAsync(
            request.Email.Trim(),
            request.Password,
            cancellationToken);

        if (credentialsResult.IsFailure)
        {
            return Result.Failure<AuthenticationResponse>(credentialsResult.Error!);
        }

        AuthenticatedUser authenticatedUser = credentialsResult.Value;
        IReadOnlyCollection<string> roles = await identityService.GetRolesAsync(
            authenticatedUser.UserId,
            cancellationToken);
        AccessToken accessToken = tokenService.GenerateAccessToken(
            authenticatedUser.UserId,
            authenticatedUser.Email,
            roles);
        RefreshTokenValue refreshTokenValue = tokenService.GenerateRefreshToken();
        RefreshTokenEntity refreshToken = RefreshTokenEntity.Issue(
            authenticatedUser.UserId,
            refreshTokenValue.Value,
            refreshTokenValue.ExpiresAtUtc,
            timeProvider.GetUtcNow());

        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);

        User user = User.Rehydrate(authenticatedUser.UserId, authenticatedUser.Email);
        user.RecordLogin();
        UserLoggedInDomainEvent domainEvent = user.DomainEvents
            .OfType<UserLoggedInDomainEvent>()
            .Single();

        await domainEventHandler.HandleAsync(domainEvent, cancellationToken);
        user.ClearDomainEvents();

        RefreshTokenIssuedDomainEvent refreshTokenIssuedDomainEvent = refreshToken.DomainEvents
            .OfType<RefreshTokenIssuedDomainEvent>()
            .Single();
        await refreshTokenIssuedEventHandler.HandleAsync(
            refreshTokenIssuedDomainEvent,
            cancellationToken);
        refreshToken.ClearDomainEvents();

        return Result.Success(new AuthenticationResponse(
            accessToken.Value,
            refreshToken.Token,
            accessToken.ExpiresAtUtc,
            user.Id,
            user.Email,
            roles));
    }
}
