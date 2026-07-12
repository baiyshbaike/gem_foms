namespace Domain.Common;

public abstract class TenantActiveSoftDeletableAuditableEntityBase : TenantSoftDeletableAuditableEntityBase, IActiveEntity
{
    public bool IsActive { get; set; } = true;
}