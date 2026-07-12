namespace Domain.Common;

public abstract class AuditableEntityBase : EntityBase, IAuditableEntity
{
    public DateTimeOffset CreatedAt { get; set; }
    public long CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
}