using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dialysis.Shared.Models
{
	public class IndicatorRow
	{

        public long Id { get; set; }
        public string? title { get; set; }
        public long IndicatorId { get; set; }
        public long ReferenceId { get; set; }
        public double? Plan1 { get; set; }
        public double? Plan2 { get; set; }
        public double? Fact1 { get; set; }
        public double? Fact2 { get; set; }
        public double? Change1 { get; set; }
        public double? Change2 { get; set; }
        public double? Threshold { get; set; }
        public long? CreatedBy { get; set; }    
        public DateTime? CreatedOn { get; set; }
        public long? LastModifiedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }


    }
}

