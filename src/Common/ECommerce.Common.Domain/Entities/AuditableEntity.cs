namespace ECommerce.Common.Domain.Entities;

public abstract class AuditableEntity<TId> : Entity<TId>
    where TId : notnull
{
    protected AuditableEntity()
    {
    }

    protected AuditableEntity(TId id)
        : base(id)
    {
    }

    public DateTimeOffset CreatedAtUtc { get; protected set; }

    public DateTimeOffset? UpdatedAtUtc { get; protected set; }

    public string? CreatedBy { get; protected set; }

    public string? UpdatedBy { get; protected set; }
}
