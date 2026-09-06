using ECommerce.Common.Application.Errors;
using ECommerce.Common.Presentation.Results;
using ECommerce.Modules.Identity.Application.Authentication.Login;
using ECommerce.Modules.Identity.Application.Authentication.Logout;
using ECommerce.Modules.Identity.Application.Authentication.RefreshToken;
using ECommerce.Modules.Identity.Application.Authentication.Register;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Modules.Identity.Presentation.Authentication;

public static class AuthenticationEndpoints
{
    public static RouteGroupBuilder MapAuthenticationEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/register", RegisterAsync).WithName("Identity.Register");
        group.MapPost("/login", LoginAsync).WithName("Identity.Login");
        group.MapPost("/refresh", RefreshAsync).WithName("Identity.Refresh");
        group.MapPost("/logout", LogoutAsync).RequireAuthorization();
        return group;
    }

    private static async Task<IResult> RefreshAsync(
        RefreshTokenRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        Result<RefreshTokenResponse> result = await sender.Send(
            new RefreshTokenCommand(request.RefreshToken),
            cancellationToken);

        return result.IsFailure
            ? ApiResults.Problem(result.Error!)
            : Results.Ok(result.Value);
    }

    private static async Task<IResult> LogoutAsync(
        LogoutRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        Result result = await sender.Send(
            new LogoutCommand(request.RefreshToken),
            cancellationToken);

        return result.IsFailure
            ? ApiResults.Problem(result.Error!)
            : Results.NoContent();
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        Result<RegisterUserResponse> result = await sender.Send(
            new RegisterUserCommand(request.Email, request.Password),
            cancellationToken);

        return result.IsFailure
            ? ApiResults.Problem(result.Error!)
            : Results.Created(
                $"/api/v1/identity/users/{result.Value.UserId}",
                result.Value);
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        Result<AuthenticationResponse> result = await sender.Send(
            new LoginUserCommand(request.Email, request.Password),
            cancellationToken);

        return result.IsFailure
            ? ApiResults.Problem(result.Error!)
            : Results.Ok(result.Value);
    }
}
