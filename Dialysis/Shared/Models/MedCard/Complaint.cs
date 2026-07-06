namespace Dialysis.Shared.Models;

public class Complaint
{
    public long Id { get; set; }
    public long PatientId { get; set; }
    public long MedCenterId { get; set; }
    public long? MedCardId { get; set; }
    public string? Inn { get; set; }
    public string TextComplaint { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }        
}