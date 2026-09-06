namespace ECommerce.Modules.Identity.Application.Authentication.RefreshToken;

public sealed record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAtUtc,
    Guid UserId,
    string Email,
    IReadOnlyCollection<string> Roles);
