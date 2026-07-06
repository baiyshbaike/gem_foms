using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dialysis.Shared.Models
{
	public class Indicator
	{
        public long Id { get; set; }
        public long MedCenterId { get; set; }
        public DateTime? startIndicator { get; set; }
        public DateTime? endIndicator { get; set; }
        public string? Title { get; set; }        
        public long? CreatedBy { get; set; }    
        public DateTime? CreatedOn { get; set; }
        public long? LastModifiedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }


    }
}

