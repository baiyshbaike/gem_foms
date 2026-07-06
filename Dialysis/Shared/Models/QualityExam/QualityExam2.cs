using System;
namespace Dialysis.Shared.Models
{
	public class QualityExam2
	{
        public long Id { get; set; }
        public DateTime? AktDate { get; set; }
        public string? Organisation { get; set; }
        public string? TuFoms { get; set; }        
        public long? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public long? LastModifiedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public long? GlobalStatusId { get; set; }
    }
}

