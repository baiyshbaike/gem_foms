using System;
namespace Dialysis.Shared.Models
{
	public class WritingOut
    {
        public long Id { get; set; }
        public long PatientId { get; set; }
        public long? MedCenterId { get; set; }
        public long? MedCardId { get; set; }
        public string? Inn { get; set; }
        public string? MedCardNumber { get; set; }
        public string? PassportNum { get; set; }
        public string? Fio { get; set; }
        public string? FIODoctor { get; set; }
        public string? FIODepartmentHead { get; set; }
        public string? Familiarized { get; set; }
        public string? Address { get; set; }        
        public DateTime? ReceiptDate { get; set; }
        public DateTime? LeaveDate { get; set; }
        public int TotalHDSession { get; set; }
        public string? BloodResus { get; set; }
        public string? MainDiagnosis { get; set; }
        public string? SoputDiagnosis { get; set; }
        public string? Complication { get; set; }

        public string? Anamnez { get; set; }
        public string? AnamnezZabol { get; set; }
        public string? Allergo { get; set; }
        public string? Gemotrans { get; set; }
        public string? GepatitB { get; set; }
        public string? Sosud { get; set; }
        public string? Objectivus { get; set; }
        public string? AllResults { get; set; }

        public string? ExamFor { get; set; }
        public string? VirusBC { get; set; }
        public string? RW { get; set; }
        public string? Vich { get; set; }
        public string? Instrumental { get; set; }
        public string? Uzi { get; set; }
        public string? Ekg { get; set; }
        public string? Eho { get; set; }
        public string? Rentgen { get; set; }
        public string? Consulting { get; set; }
        public string? Otchet { get; set; }
        public string? TimeProcedure { get; set; }
        public string? Gemo { get; set; }
        public string? Sostoyania { get; set; }
        public string? Medikamentoz { get; set; }
        public string? Gospital { get; set; }
        public string? Recommend { get; set; }
        public long? DoctorId { get; set; }
        public long? DirectorId { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public long? LastModifiedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }        
        public long? GlobalStatusId { get; set; }
        public string? ActFile { get; set; }
        public string? UniqId { get; set; }
    }
}

