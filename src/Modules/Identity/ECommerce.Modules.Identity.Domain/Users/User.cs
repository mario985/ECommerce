using ECommerce.Common.Domain.Entities;
using ECommerce.Modules.Identity.Domain.Users.Events;

namespace ECommerce.Modules.Identity.Domain.Users;

public sealed class User : AggregateRoot<Guid>
{
    private User(Guid id, string email)
        : base(id)
    {
        Email = email;
    }

    public string Email { get; }

    public static User Register(Guid id, string email)
    {
        Validate(id, email);

        User user = new(id, email.Trim());
        user.RaiseDomainEvent(new UserRegisteredDomainEvent(user.Id, user.Email));
        return user;
    }

    public static User Rehydrate(Guid id, string email)
    {
        Validate(id, email);
        return new User(id, email.Trim());
    }

    public void RecordLogin()
    {
        RaiseDomainEvent(new UserLoggedInDomainEvent(Id));
    }

    public void RecordRoleAssigned(string role, DateTimeOffset occurredAtUtc) =>
        RaiseDomainEvent(new UserRoleAssignedDomainEvent(Id, role, occurredAtUtc));
    public void RecordRoleRemoved(string role, DateTimeOffset occurredAtUtc) =>
        RaiseDomainEvent(new UserRoleRemovedDomainEvent(Id, role, occurredAtUtc));
    public void RecordDisabled(DateTimeOffset occurredAtUtc) =>
        RaiseDomainEvent(new UserDisabledDomainEvent(Id, occurredAtUtc));
    public void RecordEnabled(DateTimeOffset occurredAtUtc) =>
        RaiseDomainEvent(new UserEnabledDomainEvent(Id, occurredAtUtc));

    private static void Validate(Guid id, string email)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A user ID is required.", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(email);
    }
}
