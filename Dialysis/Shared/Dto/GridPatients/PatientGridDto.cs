namespace Dialysis.Shared.Dto.GridPatients;

public class PatientGridDto
{
    public long Id { get; set; }
    public string? Inn { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }
    public string? Address { get; set; }
    public string? Address2 { get; set; }
    public string? Phone { get; set; }
    public string? Phone2 { get; set; }        
    public long DistrictId { get; set; }
    public long RegionId { get; set; }
    public long? MedCenterId { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public long? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public long? GroupId { get; set; }
    public long? StatusId { get; set; }
    public string? GroupText { get; set; }
    public bool Gender { get; set; }
    public bool IsActive { get; set; }
    public DateTime? BirthDate { get; set; }
    public long? GroupActId { get; set; }
    public string? Image1 { get; set; }
    public string? Image2 { get; set; }
    public string? Image { get; set;}
    public string? GroupReason { get; set; }
}