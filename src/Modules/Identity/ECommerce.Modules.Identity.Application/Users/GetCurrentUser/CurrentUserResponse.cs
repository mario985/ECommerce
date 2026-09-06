namespace ECommerce.Modules.Identity.Application.Users.GetCurrentUser;

public sealed record CurrentUserResponse(
    Guid Id,
    string Email,
    IReadOnlyCollection<string> Roles);
