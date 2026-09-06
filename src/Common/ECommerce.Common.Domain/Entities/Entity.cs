using System.Runtime.CompilerServices;

namespace ECommerce.Common.Domain.Entities;

public abstract class Entity<TId>
    where TId : notnull
{
    protected Entity()
    {
    }

    protected Entity(TId id)
    {
        Id = id;
    }

    public TId Id { get; protected set; } = default!;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is not Entity<TId> other || GetType() != other.GetType())
        {
            return false;
        }

        return !IsIdDefault() && !other.IsIdDefault() &&
               EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode()
    {
        return IsIdDefault()
            ? RuntimeHelpers.GetHashCode(this)
            : HashCode.Combine(GetType(), Id);
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        return left is null ? right is null : left.Equals(right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !(left == right);
    }

    private bool IsIdDefault()
    {
        return EqualityComparer<TId>.Default.Equals(Id, default!);
    }
}
