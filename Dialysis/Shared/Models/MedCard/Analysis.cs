using System;
namespace Dialysis.Shared.Models
{
	public class Analysis
	{
        public long Id { get; set; }
        public string? Title { get; set; }
        public string? AnalysisExt { get; set; }
        public string? Description { get; set; }
        public string? Note { get; set; }
        public string? Note2 { get; set; }
        public long? NextDays { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public long? LastModifiedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }        
        public long? GlobalStatusId { get; set; }
    }
}

