namespace Domain.Common;

public abstract class TenantAuditableEntityBase : AuditableEntityBase, ITenantEntity
{
    public string TenantId { get; set; } = string.Empty;
}