namespace ECommerce.Modules.Identity.Domain.RefreshTokens;

public static class RefreshTokenErrors
{
    public const string InvalidCode = "Identity.InvalidRefreshToken";
    public const string InvalidDescription = "The refresh token is invalid.";
    public const string ExpiredCode = "Identity.ExpiredRefreshToken";
    public const string ExpiredDescription = "The refresh token has expired.";
    public const string RevokedCode = "Identity.RevokedRefreshToken";
    public const string RevokedDescription = "The refresh token has been revoked.";
    public const string NotOwnedCode = "Identity.RefreshTokenNotOwnedByUser";
    public const string NotOwnedDescription = "The refresh token is invalid.";
}
