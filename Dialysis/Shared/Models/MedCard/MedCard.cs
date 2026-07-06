using System;
namespace Dialysis.Shared.Models
{
	public class MedCard
	{
        public long Id { get; set; }
        public long PatientId { get; set; }
        public long MedCenterId { get; set; }
        public string? Inn { get; set; }
        public string? MedCardNumber { get; set; }
        public string? PassportNum { get; set; }
        public string? ConductedHemodialysis { get; set; }
        public string? FIODoctor { get; set; }
        public string? FIODepartmentHead { get; set; }
        public string? Familiarized { get; set; }
        public string? M3 { get; set; }
        public TimeSpan? ReceiptTime { get; set; }
        public TimeSpan? DirectionTime{ get; set; }
        public DateTime? DirectionDate { get; set; }
        public DateTime? ReceiptDate { get; set; }
        public DateTime? LeaveDate { get; set; }
        public int TotalHDSession { get; set; }
        public string? Blood { get; set; }
        public string? Resus { get; set; }
        public string? AllergoAnamez { get; set; }
        public string? MainDiagnosis { get; set; }
        public string? Complication1 { get; set; }
        public string? Complication2 { get; set; }
        public string? Complication3 { get; set; }
        public string? Diagnosis1 { get; set; }
        public string? Diagnosis2 { get; set; }
        public string? Diagnosis3 { get; set; }
        public string? OtherAB { get; set; }

        public TimeSpan? MainDiagnosisTime { get; set; }
        public TimeSpan? Complication1Time { get; set; }
        public TimeSpan? Complication2Time { get; set; }
        public TimeSpan? Complication3Time { get; set; }
        public TimeSpan? Diagnosis1Time { get; set; }
        public TimeSpan? Diagnosis2Time { get; set; }
        public TimeSpan? Diagnosis3Time { get; set; }
        public TimeSpan? OtherABTime { get; set; }

        public DateTime? MainDiagnosisDate { get; set; }
        public DateTime? Complication1Date { get; set; }
        public DateTime? Complication2Date { get; set; }
        public DateTime? Complication3Date { get; set; }
        public DateTime? Diagnosis1Date { get; set; }
        public DateTime? Diagnosis2Date { get; set; }
        public DateTime? Diagnosis3Date { get; set; }
        public DateTime? OtherABDate { get; set; }

        public string? MainDiagnosisCode { get; set; }
        public string? Complication1Code { get; set; }
        public string? Complication2Code { get; set; }
        public string? Complication3Code { get; set; }
        public string? Diagnosis1Code { get; set; }
        public string? Diagnosis2Code { get; set; }
        public string? Diagnosis3Code { get; set; }
        public string? OtherABCode { get; set; }
        public string? OutTreatment { get; set; }

        public string? Title { get; set; }
        public string? OutcomeTreatment { get; set; }
        public string? Pedikulez { get; set; }
        public string? Chesotka { get; set; }
        public string? Vassermana { get; set; }
        public string? Fluorografia { get; set; }
        public string? Alchohol { get; set; }
        public string? Vgv { get; set; }
        public DateTime? VgvDate { get; set; }
        public string? Vgs { get; set; }
        public DateTime? VgsDate { get; set; }
        public string? Vich { get; set; }
        public DateTime? VichDate { get; set; }
        public string? Note { get; set; }
        public string? Obosnovanie { get; set; }
        public string? Plan { get; set; }
        public string? IndividualPlan { get; set; }
        public string? Recommendation { get; set; }
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

        //public virtual ICollection<MedSession> MedSessions { get; set; }
        //public virtual ICollection<GDSession> GDSessions { get; set; }
        //public virtual Patient Patient { get; set; }
    }
}

