namespace Domain.Common;

public abstract class ActiveSoftDeletableAuditableEntityBase : SoftDeletableAuditableEntityBase, IActiveEntity
{
    public bool IsActive { get; set; } = true;
}