using System.Security.Cryptography;
using ECommerce.Modules.Identity.Application.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.Modules.Identity.Infrastructure.Authentication;

internal sealed class RefreshTokenGenerator(
    IOptions<JwtOptions> options,
    TimeProvider timeProvider)
{
    private const int TokenByteLength = 64;
    private readonly JwtOptions _options = options.Value;

    public RefreshTokenValue Generate()
    {
        byte[] randomBytes = RandomNumberGenerator.GetBytes(TokenByteLength);
        string token = Base64UrlEncoder.Encode(randomBytes);
        DateTimeOffset expiresAtUtc = timeProvider
            .GetUtcNow()
            .AddDays(_options.RefreshTokenExpirationDays);

        return new RefreshTokenValue(token, expiresAtUtc);
    }
}
