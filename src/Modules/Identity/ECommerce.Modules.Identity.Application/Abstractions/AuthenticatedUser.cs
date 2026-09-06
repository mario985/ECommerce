namespace ECommerce.Modules.Identity.Application.Abstractions;

public sealed record AuthenticatedUser(Guid UserId, string Email);
