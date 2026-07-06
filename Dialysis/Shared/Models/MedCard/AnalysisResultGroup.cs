using System;
namespace Dialysis.Shared.Models
{
	public class AnalysisResultGroup
    {
        public long Id { get; set; }
        public string? Inn { get; set; }
        public long? PatientId { get; set; }
        public long MedCardId { get; set; }        
        public string? Title { get; set; }
        public string? Note { get; set; }
       
        public string? UploadFile1 { get; set; }
        public string? UploadFile2 { get; set; }
        public string? UploadFile3 { get; set; }
        public string? UploadFile4 { get; set; }
        public string? UploadFile5 { get; set; }
        public string? UploadFile6 { get; set; }
        public string? UploadFile7 { get; set; }
        public string? UploadFile8 { get; set; }
        public string? UploadFile9 { get; set; }
        public string? UploadFile10 { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public long? LastModifiedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public DateTime? AnalysisDate { get; set; }
    }
}

