namespace Domain.Common;

public abstract class SoftDeletableAuditableEntityBase : AuditableEntityBase, ISoftDeletableEntity
{
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}