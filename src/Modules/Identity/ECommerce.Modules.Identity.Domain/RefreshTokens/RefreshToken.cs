using ECommerce.Common.Domain.Entities;
using ECommerce.Modules.Identity.Domain.Users.Events;

namespace ECommerce.Modules.Identity.Domain.RefreshTokens;

public sealed class RefreshToken : AggregateRoot<Guid>
{
    private RefreshToken()
    {
        Token = string.Empty;
    }

    private RefreshToken(
        Guid id,
        Guid userId,
        string token,
        DateTimeOffset expiresAtUtc,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        UserId = userId;
        Token = token;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid UserId { get; private set; }

    public string Token { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public string? ReplacedByToken { get; private set; }

    public static RefreshToken Issue(
        Guid userId,
        string token,
        DateTimeOffset expiresAtUtc,
        DateTimeOffset createdAtUtc)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("A user ID is required.", nameof(userId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        if (expiresAtUtc <= createdAtUtc)
        {
            throw new ArgumentException(
                "Refresh-token expiration must be later than its creation time.",
                nameof(expiresAtUtc));
        }

        RefreshToken refreshToken = new(
            Guid.NewGuid(),
            userId,
            token,
            expiresAtUtc,
            createdAtUtc);
        refreshToken.RaiseDomainEvent(new RefreshTokenIssuedDomainEvent(userId));
        return refreshToken;
    }

    public bool IsExpired(DateTimeOffset utcNow) => utcNow >= ExpiresAtUtc;

    public bool IsActive(DateTimeOffset utcNow) =>
        RevokedAtUtc is null && !IsExpired(utcNow);

    public RefreshTokenStatus GetStatus(DateTimeOffset utcNow)
    {
        if (RevokedAtUtc is not null)
        {
            return RefreshTokenStatus.Revoked;
        }

        return IsExpired(utcNow)
            ? RefreshTokenStatus.Expired
            : RefreshTokenStatus.Active;
    }

    public bool Revoke(
        DateTimeOffset revokedAtUtc,
        string? replacedByToken = null)
    {
        if (RevokedAtUtc is not null)
        {
            return false;
        }

        if (revokedAtUtc < CreatedAtUtc)
        {
            throw new ArgumentException(
                "A refresh token cannot be revoked before it was created.",
                nameof(revokedAtUtc));
        }

        RevokedAtUtc = revokedAtUtc;
        ReplacedByToken = string.IsNullOrWhiteSpace(replacedByToken)
            ? null
            : replacedByToken;
        RaiseDomainEvent(new RefreshTokenRevokedDomainEvent(UserId));
        return true;
    }
}
