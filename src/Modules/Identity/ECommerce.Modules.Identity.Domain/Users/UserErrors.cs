namespace ECommerce.Modules.Identity.Domain.Users;

public static class UserErrors
{
    public const string DuplicateEmailCode = "Identity.DuplicateEmail";
    public const string DuplicateEmailDescription = "A user with this email address already exists.";
    public const string InvalidCredentialsCode = "Identity.InvalidCredentials";
    public const string InvalidCredentialsDescription = "The email address or password is incorrect.";
    public const string UserNotFoundCode = "Identity.UserNotFound";
    public const string UserNotFoundDescription = "The user account could not be found.";
    public const string RoleNotFoundCode = "Identity.RoleNotFound";
    public const string InvalidRoleCode = "Identity.InvalidRole";
    public const string CannotDisableSelfCode = "Identity.CannotDisableSelf";
    public const string CannotRemoveOwnAdminRoleCode = "Identity.CannotRemoveOwnAdminRole";
    public const string CannotRemoveFinalAdminCode = "Identity.CannotRemoveFinalAdmin";
    public const string CannotRemoveLastRoleCode = "Identity.CannotRemoveLastRole";
}
