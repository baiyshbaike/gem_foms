namespace Dialysis.Shared.Models.Files;

public class MedCenterPatientFile
{
    public long Id { get; set; }
    public long PatientId { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string FileType { get; set; }
    public string OriginalFileName { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadedOn { get; set; }
    public long UploadedBy { get; set; }
    public bool IsDeleted { get; set; }
    public long? UploadedByUserId { get; set; }
    public long? MedCenterId { get; set; }
    public User? UploadedByUser { get; set; }
    public MedCenter? MedCenter { get; set; }
    public virtual Patient Patient { get; set; }
}