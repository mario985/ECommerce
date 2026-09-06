namespace ECommerce.Common.Application.Authentication;

public interface ICurrentUser
{
    Guid? UserId { get; }

    bool IsAuthenticated { get; }

    string? Email { get; }

    IReadOnlyCollection<string> Roles { get; }
}
