namespace ECommerce.Modules.Identity.Application.Authentication.Login;

public sealed record AuthenticationResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAtUtc,
    Guid UserId,
    string Email,
    IReadOnlyCollection<string> Roles);
