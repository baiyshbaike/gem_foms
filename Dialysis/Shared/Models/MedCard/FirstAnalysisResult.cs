using System;
namespace Dialysis.Shared.Models
{
	public class FirstAnalysisResult
    {
        public long Id { get; set; }       
        public long MedCardId { get; set; }
        public long? PatientId { get; set; }
        public long AnalysisId { get; set; }
        public long? FirstAnalysisId { get; set; }
        public string? AnalysisName { get; set; }
        public double? Result { get; set; }
        public string? Status { get; set; }      
        public string? Note { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public long? LastModifiedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public DateTime? AnalysisDate { get; set; }
        public string? UniqId { get; set; }
    }
}

