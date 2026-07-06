namespace Dialysis.Shared.Models;

public class MedCenterEmployee
{
    public long Id { get; set; }
    public long MedCenterId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }
    public string? FullName { get; set; }
    public string? JobTitle { get; set; }
    public string? PhoneNumber { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime? CreatedOn { get; set; }
    public long? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public bool IsActive { get; set; } = true;
    public string? UpFile1 { get; set; }
    public string? FileContent { get; set; } 
}