using System;
namespace Dialysis.Shared.Models
{
	public class QualityExam1
	{
        public long Id { get; set; }
        public DateTime? AktDate { get; set; }
        public string? Organisation { get; set; }
        public string? Patient { get; set; }
        public string? PatientPin { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public long? HD { get; set; }
        public long? HDF { get; set; }
        public long? UF { get; set; }
        public bool? Lab { get; set; }
        public bool? Instrument { get; set; }
        public bool? Consulting { get; set; }
        public bool? After { get; set; }
        public bool? Iron { get; set; }
        public bool? Fistul { get; set; }
        public long? Achieved { get; set; }
        public long? NotAchieved { get; set; }
        public long? TotalMonth { get; set; }
        public bool? Dose { get; set; }
        public bool? Hb { get; set; }
        public bool? Ferrytip { get; set; }
        public bool? Fosfat { get; set; }
        public bool? Calcium { get; set; }
        public bool? ParatGram { get; set; }
        public bool? Albumin { get; set; }
        public bool? Ad { get; set; }
        public bool? Document { get; set; }
        public bool? AllParams { get; set; }
        public string? Conclusion { get; set; }
        public string? Recommendations { get; set; }       
        public long? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public long? LastModifiedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }        
        public long? GlobalStatusId { get; set; }
    }
}

