using System;
namespace Dialysis.Shared.Models
{
	public class MedCenterFiles
	{
        public long Id { get; set; }
        public long MedCenterId { get; set; }
        public string? Name { get; set; }
        public string? Extension { get; set; }
        public string? FileUrl { get; set; }
        public string? IsDownload { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public long? LastModifiedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool IsActive { get; set; }
    }
}

