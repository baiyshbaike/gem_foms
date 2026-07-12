namespace Domain.Common;

public abstract class TenantSoftDeletableAuditableEntityBase : TenantAuditableEntityBase, ISoftDeletableEntity
{
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}