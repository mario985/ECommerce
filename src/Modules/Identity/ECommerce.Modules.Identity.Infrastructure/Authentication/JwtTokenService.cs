using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerce.Modules.Identity.Application.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.Modules.Identity.Infrastructure.Authentication;

internal sealed class JwtTokenService(
    IOptions<JwtOptions> options,
    TimeProvider timeProvider,
    RefreshTokenGenerator refreshTokenGenerator)
    : ITokenService
{
    private readonly JwtOptions _options = options.Value;

    public AccessToken GenerateAccessToken(
        Guid userId,
        string email,
        IReadOnlyCollection<string> roles)
    {
        DateTimeOffset issuedAtUtc = timeProvider.GetUtcNow();
        DateTimeOffset expiresAtUtc = issuedAtUtc.AddMinutes(_options.ExpirationMinutes);

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        ];
        claims.AddRange(roles.Select(role => new Claim("role", role)));

        SigningCredentials credentials = new(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
            SecurityAlgorithms.HmacSha256);
        JwtSecurityToken token = new(
            _options.Issuer,
            _options.Audience,
            claims,
            notBefore: issuedAtUtc.UtcDateTime,
            expires: expiresAtUtc.UtcDateTime,
            credentials);

        return new AccessToken(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAtUtc);
    }

    public RefreshTokenValue GenerateRefreshToken() => refreshTokenGenerator.Generate();
}
