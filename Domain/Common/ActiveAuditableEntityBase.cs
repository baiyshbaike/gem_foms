namespace Domain.Common;

public abstract class ActiveAuditableEntityBase : AuditableEntityBase, IActiveEntity
{
    public bool IsActive { get; set; } = true;
}