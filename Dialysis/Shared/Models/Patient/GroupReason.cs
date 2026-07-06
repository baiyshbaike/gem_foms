using System;
namespace Dialysis.Shared.Models
{
	public class GroupReason
    {
        public long Id { get; set; }  
        public string? Title { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}

