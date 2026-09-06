namespace ECommerce.Modules.Identity.Application.Abstractions;

public interface ITokenService
{
    AccessToken GenerateAccessToken(
        Guid userId,
        string email,
        IReadOnlyCollection<string> roles);

    RefreshTokenValue GenerateRefreshToken();
}

public sealed record AccessToken(string Value, DateTimeOffset ExpiresAtUtc);

public sealed record RefreshTokenValue(string Value, DateTimeOffset ExpiresAtUtc);
