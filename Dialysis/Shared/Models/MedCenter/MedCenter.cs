using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dialysis.Shared.Models
{
	public class MedCenter
	{
        public long Id { get; set; }
        public string? Title { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Phone2 { get; set; }
        public long DistrictId { get; set; }
        public long RegionId { get; set; }     
        public long? CreatedBy { get; set; }    
        public DateTime? CreatedOn { get; set; }
        public long? LastModifiedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }


    }
}

