using System;
namespace Dialysis.Shared.Models
{
	public class HDSession
	{
        public long Id { get; set; }
        public string? Inn { get; set; }
        public long PatientId { get; set; }
        public long? MedCardId { get; set; }
        public long? MedCenterId { get; set; }
        public long? MachineId { get; set; }
        public string? Condition { get; set; }
        public string? Complaints { get; set; }
        public string? Program { get; set; }
        public string? Dialyzer { get; set; }
        public string? Correction { get; set; }
        public string? Reinfusion { get; set; }
        public string? Access { get; set; }
        public string? Anticoagulation { get; set; }
        public string? Uf { get; set; }
        public string? Speed { get; set; }
        public string? TypeDialyzer { get; set; }
        public string? TypeMedicine { get; set; }
        public string? Durators { get; set; }
        public double? StartWeight { get; set; }
        public double? EndWeight { get; set; }
        public string? Note { get; set; }
        public long? IdentifyBy { get; set; }
        public DateTime? IdentifyOn { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public long? FinishedBy { get; set; }
        public DateTime? FinishedOn { get; set; }
        public long? LastModifiedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public DateTime? SessionStart { get; set; }
        public DateTime? SessionEnd { get; set; }
        public long? StatusId { get; set; }
        public double? TotalMinutes { get; set; }
        public double? TotalHours { get; set; }        
        public decimal? ActivePrice { get; set; }
        public decimal? SetPrice { get; set; }
        public string? Note2 { get; set; }
        public string? Note3 { get; set; }
        public string? ImageStart { get; set; }
        public string? ImageEnd { get; set; }
        public string? Sys { get; set; }
        public string? Dia { get; set; }
        public string? Temp { get; set; }
        public string? Ritm { get; set; }

        public string? EndSys { get; set; }
        public string? EndDia { get; set; }
        public string? EndTemp { get; set; }
        public string? EndRitm { get; set; }
		
        public bool Сhanged { get; set; }=false;
        public bool? Hd1Hypotension { get; set; }
        public bool? Hd2Hypertension { get; set; }
        public bool? Hd3MuscleCrampsOfTheLimbs { get; set; }
        public bool? Hd4HeartRhythmDisturbances { get; set; }
        public bool? Hd5Headache { get; set; }
        public bool? Hd6AnginaAttacks { get; set; }
        public string? Hd7Other { get; set; }
        public string? Hd8CorrectionOfComplications { get; set; }
        public string? Hd9PlannedAppointments { get; set; }
        public string? Hd10Recommendations { get; set; }
        public string? Hd11EffectiveTime { get; set; }
        public string? FioDoctor { get; set; }
        public string? FioDepartmentHead { get; set; }
        public bool? SendTunduk { get; set; } = false;
	}
}

