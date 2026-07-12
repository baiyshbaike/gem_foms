namespace Domain.Common;

public interface IAuditableEntity
{
    DateTimeOffset CreatedAt { get; set; }
    long CreatedBy { get; set; }
    DateTimeOffset? UpdatedAt { get; set; }
    long? UpdatedBy { get; set; }
}